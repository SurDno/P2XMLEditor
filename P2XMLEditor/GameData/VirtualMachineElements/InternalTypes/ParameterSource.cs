using System;
using System.Collections.Generic;
using System.Linq;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.Enums;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.GameData.VirtualMachineElements.Interfaces;
using P2XMLEditor.GameData.VirtualMachineElements.Placeholders;
using P2XMLEditor.Helper;
using P2XMLEditor.Logging;
using VmType = P2XMLEditor.GameData.Enums.VmType;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes;

public struct ParameterSource {
	public ParameterValue? LiteralValue { get; set; }
	public bool IsConstant { get; set; }
	public VmTypeInfo TypeInfo { get; set; } = VmTypeInfo.Unknown;
	public Message? MessageReference { get; set; }
	public InputParameter? InputParamReference { get; set; }
	public ParameterHolder? PrefixHolder { get; set; }
	public HierarchyGuid? PrefixHierarchy { get; set; }
	public InputParameter? PrefixInputParamReference { get; set; }
	public string? PrefixString { get; set; }
	public Parameter? ParameterReference { get; set; }
	public VmElement? ElementReference { get; set; }
	public HierarchyGuid? HierarchyReference { get; set; }
	public Parameter? DynamicObjectReference { get; set; }
	public string? DynamicParameterName { get; set; }
	public BlueprintRef? BlueprintReference { get; set; }
	public EntityRef? EntityReference { get; set; }
	public ActionLine? LoopActionLine { get; set; }
	public bool IsLoopIndex { get; set; }
	public bool IsLoopElement { get; set; }
	public string? LoopListName { get; set; }
	public string? GlobalListName { get; set; }
	public ulong? GlobalListTargetId { get; set; }
	public bool HasLeadingPercent { get; set; }

	
	// for literal values - sometimes serialized as "0.5", sometimes as "0,5". 
	// no difference for the editor or the engine, just to ensure it researilizes the same
	public bool IsCommaSeparator { get; set; }
	
	/// <summary>
	/// True when a single hierarchy reference was serialized as "A%A".
	/// Set instead of PrefixHierarchy, so the hierarchy is not stored twice.
	/// </summary>
	public bool HierarchyWrittenTwice { get; set; }

	public ParameterSource() { }

	public static ParameterSource Create(string data, VirtualMachine vm, ParamTarget? target = null,
		VmTypeInfo? expectedType = null) {
		var src = new ParameterSource();

		if (data.StartsWith("const_")) {
			src.LiteralValue = ParameterValue.Create(vm, VmTypeInfo.Int32, data[6..]);
			src.IsConstant = true;
			return src;
		}

		src.HasLeadingPercent = data.StartsWith('%');
		var body = src.HasLeadingPercent ? data[1..] : data;

		// --- (2) hierarchy written twice: "A%A" is one reference, not prefix + value.
		if (HierarchyGuid.TryParseDoubled(body, vm, out var doubled)) {
			src.HierarchyReference = doubled;
			src.HierarchyWrittenTwice = true;
			src.ElementReference = doubled!.Elements[^1].Element;
			src.TypeInfo = expectedType ?? VmTypeInfo.GameObject;
			ApplyRefWrapper(ref src, vm, expectedType);
			return src;
		}

		// --- prefix / content split
		var sep = body.IndexOf('%');
		string content;
		if (sep != -1) {
			var prefix = body[..sep];
			content = body[(sep + 1)..];

			if (ulong.TryParse(prefix, out var prefixId)) {
				var prefixElement = vm.GetNullableElement<VmElement>(prefixId);
				if (prefixElement is Parameter dynamicObjectReference) {
					src.DynamicObjectReference = dynamicObjectReference;
					src.DynamicParameterName = content;
				} else if (prefixElement is ParameterHolder prefixHolder) {
					src.PrefixHolder = prefixHolder;
				}
			} else if (HierarchyGuid.TryParse(prefix, vm, out var prefixHierarchy)) {
				src.PrefixHierarchy = prefixHierarchy;
			} else {
				src.PrefixString = prefix;
				Logger.Log(LogLevel.Warning, $"Using string prefix for: {data}");
			}
		} else {
			content = body;
		}

		if (content.Length == 0) {
			src.TypeInfo = expectedType ?? VmTypeInfo.Unknown;
			return src;
		}
		
		// --- (1) bare hierarchy value, with or without a prefix.
		if (HierarchyGuid.TryParse(content, vm, out var hierarchyValue)) {
			HitTracker.Hit(data);
			src.HierarchyReference = hierarchyValue;
			src.ElementReference = hierarchyValue!.Elements[^1].Element;
			src.TypeInfo = expectedType ?? VmTypeInfo.GameObject;
			ApplyRefWrapper(ref src, vm, expectedType);
			return src;
		}

		var holder = src.PrefixHolder;
		if (holder == null && src.PrefixHierarchy != null) {
			holder = src.PrefixHierarchy.Elements[^1].Element as ParameterHolder;
		}

		if (content.Contains("_message_") && !content.Contains('&')) {
			if (vm.TryResolveMessage(content, out var message)) {
				src.MessageReference = message;
				src.TypeInfo = expectedType ?? VmTypeHelper.GetVmTypeInfo(message!.Type, vm);
				return src;
			}
		} else if (content.Contains("_inputparam_")) {
			if (InputParameter.TryParse(content, out var inputParam)) {
				src.InputParamReference = inputParam;
				src.TypeInfo = expectedType ?? VmTypeHelper.GetVmTypeInfo(inputParam!.Type, vm);
				return src;
			}
		} else if (content.Contains("_Loop_")) {
			HitTracker.Hit(data);
			ParseLoopVariable(ref src, content, vm);
		} else if (content.StartsWith("global_")) {
			HitTracker.Hit(data);
			ParseGlobalVariable(ref src, content);
		} else if (ulong.TryParse(content, out var contentId) && contentId >= IdGenerator.MIN_ID) {
			HitTracker.Hit(data);
			// Resolve WITHOUT a type constraint first.
			// "<holderId>%<parameterId>" is an indirection: the value is whatever that
			// Parameter points at, and it is legal in IObjRef / IBlueprintRef / IEntity
			// slots. The old code called GetNullableElement<GameObject>(id) directly,
			// which throws when the id resolves to a Parameter.
			var contentElement = vm.GetNullableElement(contentId);

			if (contentElement is Parameter parameterReference) {
				HitTracker.Hit(data);
				src.ParameterReference = parameterReference;
			} else switch (expectedType) {
				case { BaseType: VmType.BlueprintRef or VmType.BlueprintRefStorable }:
					src.BlueprintReference = contentElement switch {
						Item item => new() { Element = new(item), SerializeAsGuid = false },
						Other other => new() { Element = new(other), SerializeAsGuid = false },
						Character character => new() { Element = new(character), SerializeAsGuid = false },
						_ => src.BlueprintReference
					};
					if (contentElement != null) 
						src.ElementReference = contentElement;
					break;
				case { BaseType: VmType.EntityRef }:
					src.EntityReference = new() { Element = contentElement as ParameterHolder, SerializeAsGuid = false };
					if (contentElement != null) 
						src.ElementReference = contentElement;
					break;
				default: {
					if (contentElement != null) 
						src.ElementReference = contentElement;
					break;
				}
			}
		} else switch (expectedType) {
			case { BaseType: VmType.BlueprintRef or VmType.BlueprintRefStorable }: {
				var gameObject = vm.GetElementsByType<GameObject>().FirstOrDefault(x => x.EngineTemplateId == content);
				src.BlueprintReference = gameObject switch {
					Item item => new() { Element = new(item), SerializeAsGuid = true },
					Other other => new() { Element = new(other), SerializeAsGuid = true },
					Character character => new() { Element = new(character), SerializeAsGuid = true },
					_ => src.BlueprintReference
				};
				src.ElementReference = gameObject;
				break;
			}
			case { BaseType: VmType.EntityRef }: {
				var gameObject = vm.GetElementsByType<GameObject>().FirstOrDefault(x => x.EngineTemplateId == content);
				if (gameObject != null) {
					src.EntityReference = new EntityRef { Element = gameObject, SerializeAsGuid = true };
					src.ElementReference = gameObject;
				}

				break;
			}
		}

		src.TypeInfo = expectedType ?? VmTypeInfo.Unknown;
		if (src.TypeInfo.BaseType == VmType.Unknown)
			InferTypeInfo(ref src, vm, target);

		if (!src.HasResolvedTarget()) {
			HitTracker.Hit(data);

			// Only fall back to the Unknown marker when the type really is unknown.
			// Tokens like "_message_" also occur inside opaque payloads (e.g. the
			// CONTEXT&PARAM operation-path strings the engine hands to OperationPathInfo),
			// which are ordinary System.String values and must keep their declared type.

			try {
				src.LiteralValue = ParameterValue.Create(vm, src.TypeInfo, content);
			} catch (Exception ex) {
				HitTracker.Hit(data);
				Logger.Log(LogLevel.Error,
					$"Failed to parse '{data}' as {src.TypeInfo}: {ex.Message} in {VirtualMachine.FillScope.Id}. Preserving as raw string.");
				src.LiteralValue = new BasicValue<string>(src.TypeInfo.Serialize(), content);
			}


			src.IsCommaSeparator = content.Contains(',') && src.LiteralValue is BasicValue<float>;

			if (src.TypeInfo.BaseType == VmType.Unknown)
				Logger.Log(LogLevel.Warning,
					$"Using literal parsing for {data}, guessed type:{src.TypeInfo} {src.LiteralValue?.XmlType}");
		}

		return src;
	}
	
	private static readonly HashSet<VmType> RefLikeTypes = [
		VmType.GameObject, VmType.EntityRef, VmType.BlueprintRef, VmType.BlueprintRefStorable
	];

	/// <summary>
	/// A hierarchy that lands in an IObjRef / IBlueprintRef / IEntity slot still has to
	/// populate the matching wrapper so the editor treats it as a reference.
	/// </summary>
	private static void ApplyRefWrapper(ref ParameterSource src, VirtualMachine vm, VmTypeInfo? expectedType) {
		var leaf = src.HierarchyReference!.Elements[^1].Element;

		switch (expectedType?.BaseType) {
			case VmType.BlueprintRef:
			case VmType.BlueprintRefStorable:
				HitTracker.Hit();
				if (leaf is Item item) {
					HitTracker.Hit();
					src.BlueprintReference = new() { Element = new(item), SerializeAsGuid = false };
				} else if (leaf is Other other) {
					HitTracker.Hit();
					src.BlueprintReference = new() { Element = new(other), SerializeAsGuid = false };
				} else if (leaf is Character character) {
					HitTracker.Hit();
					src.BlueprintReference = new() { Element = new(character), SerializeAsGuid = false };
				} else {
					HitTracker.Hit();
					Logger.Log(LogLevel.Warning,
						$"Hierarchy {src.HierarchyReference.Write()} resolves to {leaf?.GetType().Name ?? "null"}, " +
						$"which is neither Item nor Other; BlueprintRef left unset.");
				}
				break;

			case VmType.EntityRef:
				HitTracker.Hit();
				src.EntityReference = new EntityRef { Element = leaf as GameObject, SerializeAsGuid = false };
				break;
		}
	}

	private bool HasResolvedTarget() =>
		MessageReference != null || InputParamReference != null || ParameterReference != null ||
		ElementReference != null || IsLoopIndex || IsLoopElement || GlobalListName != null ||
		HierarchyReference != null || DynamicObjectReference != null;

	private static void InferTypeInfo(ref ParameterSource src, VirtualMachine vm, ParamTarget? target) {
		if (src.MessageReference != null) {
			HitTracker.Hit();
			src.TypeInfo = VmTypeHelper.GetVmTypeInfo(src.MessageReference.Type, vm);
			return;
		}
		if (src.InputParamReference != null) {
			HitTracker.Hit();
			src.TypeInfo = VmTypeHelper.GetVmTypeInfo(src.InputParamReference.Type, vm);
			return;
		}
		if (src.ParameterReference != null) {
			HitTracker.Hit();
			src.TypeInfo = VmTypeHelper.GetVmTypeInfo(src.ParameterReference.Type, vm);
			return;
		}
		if (src.IsLoopIndex) {
			HitTracker.Hit();
			src.TypeInfo = VmTypeInfo.Int32;
			return;
		}
		if (src is { IsLoopElement: true, LoopActionLine.LoopInfo: not null }) {
			HitTracker.Hit();
			src.TypeInfo = VmTypeInfo.GameObject;
			return;
		}
		if (src.HierarchyReference != null) {
			HitTracker.Hit();
			src.TypeInfo = VmTypeInfo.GameObject;
			return;
		}
		if (src.GlobalListName != null) {
			HitTracker.Hit();
			src.TypeInfo = new VmTypeInfo(VmType.List) { UnderlyingType = VmTypeInfo.GameObject };
			return;
		}
		if (target == null || target.Value.Kind == ParamTargetKind.Empty) {
			HitTracker.Hit();
			return;
		}
		if (target.Value.Parameter?.Element is Parameter variable) {
			HitTracker.Hit();
			src.TypeInfo = VmTypeHelper.GetVmTypeInfo(variable.Value.XmlType, vm);
			return;
		}
		if (target.Value.ComponentParamName is { } componentParam &&
		    vm.TryResolveStandartParamType(componentParam, out var componentType)) {
			HitTracker.Hit();
			src.TypeInfo = componentType;
			return;
		}
		//HitTracker.Hit();
	}

	private static void ParseLoopVariable(ref ParameterSource source, string content, VirtualMachine vm) {
		var array = content.Split('_');
		ulong.TryParse(array[1], out var actionLineId);
		
		
		

		HitTracker.Hit(content);
		source.LoopActionLine = vm.GetNullableElement<ActionLine>(actionLineId);

		if (content.EndsWith("_Index")) {
			HitTracker.Hit(content);
			source.IsLoopIndex = true;
		} else if (content.EndsWith("_Element")) {
			HitTracker.Hit(content);
			source.IsLoopElement = true;
			var listIdx = content.IndexOf("_List_", StringComparison.Ordinal);
			var elementIdx = content.LastIndexOf("_Element", StringComparison.Ordinal);
			if (listIdx != -1 && elementIdx != -1 && elementIdx > listIdx + 6) {
				HitTracker.Hit(content);
				source.LoopListName = content[(listIdx + 6)..elementIdx];
			}/* else {
				HitTracker.Hit(content);
			}
		} else {
			HitTracker.Hit(content);*/
		}
	}

	private static void ParseGlobalList(ref ParameterSource source, string content) {
		var idx = content.LastIndexOf('_');
		if (idx == -1) {
			HitTracker.Hit(content);
			return;
		}

		HitTracker.Hit(content);
		source.GlobalListName = content[..idx];
		if (ulong.TryParse(content[(idx + 1)..], out var targetId)) {
			HitTracker.Hit(content);
			source.GlobalListTargetId = targetId;
		} else {
			HitTracker.Hit(content);
			// No trailing id: the whole string is the list name.
			source.GlobalListName = content;
		}
	}

	private static void ParseGlobalVariable(ref ParameterSource source, string content) {
		HitTracker.Hit(content);
		source.GlobalListName = content;
	}

	public string? GetVariableName() {
		if (ParameterReference != null) return ParameterReference.Name;
		if (ElementReference is INamedElement namedElement) return namedElement.Name;
		if (DynamicParameterName != null) return DynamicParameterName;
		if (HierarchyReference != null) return HierarchyReference.Write();
		if (GlobalListName != null) return GlobalListName;
		if (MessageReference != null) return MessageReference.Name;
		if (InputParamReference != null) return InputParamReference.Name;

		if (LiteralValue != null) {
			var text = LiteralValue.Serialize();
			return IsConstant ? "const_" + text : text;
		}

		return null;
	}

	public string Write() {
		if (DynamicObjectReference != null && DynamicParameterName != null)
			return $"{DynamicObjectReference.Id}%{DynamicParameterName}";

		var prefix = PrefixHolder != null                ? PrefixHolder.Id.ToString()
				   : PrefixHierarchy != null             ? PrefixHierarchy.Write()
				   : PrefixInputParamReference != null   ? PrefixInputParamReference.Name
				   : PrefixString;

		// "A%A" round-trips as written, including any leading percent.
		if (HierarchyWrittenTwice && HierarchyReference != null) {
			var hierarchy = HierarchyReference.Write();
			return Compose(prefix, $"{hierarchy}%{hierarchy}");
		}

		if (LiteralValue != null) {
			var literal = LiteralValue.Serialize();
			if (IsCommaSeparator) literal = literal.Replace('.', ',');

			if (string.Equals(literal, "none", StringComparison.OrdinalIgnoreCase))
				return Compose(prefix, literal);

			if (IsConstant) {
				var constant = "const_" + literal;
				return prefix != null ? $"{prefix}%{constant}" : constant;
			}

			return Compose(prefix, literal);
		}

		string? value = null;
		if (IsLoopIndex) {
			value = $"local_{LoopActionLine?.Id}_Loop_Index";
		} else if (IsLoopElement) {
			var listName = LoopActionLine?.LoopInfo?.Name.GetVariableName() ?? LoopListName;
			value = $"local_{LoopActionLine?.Id}_Loop_List_{listName}_Element";
		} else if (GlobalListName != null) {
			value = GlobalListTargetId.HasValue ? $"{GlobalListName}_{GlobalListTargetId}" : GlobalListName;
		} else if (MessageReference != null) {
			value = MessageReference.Name;
		} else if (InputParamReference != null) {
			value = InputParamReference.Name;
		} else if (ParameterReference != null) {
			value = ParameterReference.Id.ToString();
			prefix ??= ParameterReference.Parent.Id.ToString();
		} else if (HierarchyReference != null) {
			value = HierarchyReference.Write();
		} else if (BlueprintReference != null) {
			value = BlueprintReference.SerializeAsGuid
				? (BlueprintReference.Element.Element as GameObject)?.EngineTemplateId
				: BlueprintReference.Element.Element?.Id.ToString();
		} else if (EntityReference != null) {
			value = EntityReference.SerializeAsGuid
				? ((GameObject)EntityReference.Element!).EngineTemplateId
				: EntityReference.Element?.Id.ToString();
		} else if (ElementReference != null) {
			value = ElementReference.Id.ToString();
		}

		return Compose(prefix, value);
	}

	private readonly string Compose(string? prefix, string? value) {
		if (prefix != null)
			return HasLeadingPercent ? $"%{prefix}%{value}" : $"{prefix}%{value}";
		if (value == null)
			return HasLeadingPercent ? "%" : "";
		return HasLeadingPercent ? "%" + value : value;
	}

	public override string ToString() => Write();
}
