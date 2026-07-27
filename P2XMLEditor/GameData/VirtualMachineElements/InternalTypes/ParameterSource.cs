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
	public MessageInfo? MessageReference { get; set; }
	public GraphParamInfo? InputParamReference { get; set; }
	public VmEither<Graph, GraphPlaceholder>? InputParamGraphOwner { get; set; }
	public ParameterHolder? PrefixHolder { get; set; }
	public HierarchyGuid? PrefixHierarchy { get; set; }
	public GraphParamInfo? PrefixInputParamReference { get; set; }
	public Graph? PrefixInputParamGraphOwner { get; set; }
	public string? PrefixString { get; set; }
	public Parameter? ParameterReference { get; set; }
	public VmElement? ElementReference { get; set; }
	public HierarchyGuid? HierarchyReference { get; set; }
	public Parameter? DynamicObjectReference { get; set; }
	public string? DynamicParameterName { get; set; }
	public BlueprintRef? BlueprintReference { get; set; }
	public EntityRef? EntityReference { get; set; }
	public bool IsCommaSeparator { get; set; }
	public ActionLine? LoopActionLine { get; set; }
	public bool IsLoopIndex { get; set; }
	public bool IsLoopElement { get; set; }
	public string? LoopListName { get; set; }
	public string? GlobalListName { get; set; }
	public ulong? GlobalListTargetId { get; set; }
	public bool HasLeadingPercent { get; set; }

	public ParameterSource() { }


	


	public static class HitTracker {
		public static bool Hit0;
		public static bool Hit1;
		public static bool Hit2;
		public static bool Hit3;
		public static bool Hit4;
		public static bool Hit5;
		public static bool Hit6;
		public static bool Hit7;
		public static bool Hit8;
		public static bool Hit9;
		public static bool Hit10;
		public static bool Hit11;
		public static bool Hit12;
		public static bool Hit13;
		public static bool Hit14;
		public static bool Hit15;
		public static bool Hit16;
		public static bool Hit17;
		public static bool Hit18;
		public static bool Hit19;
		public static bool Hit20;
		public static bool Hit21;
		public static bool Hit22;
		public static bool Hit23;
		public static bool Hit24;
		public static bool Hit25;
		public static bool Hit26;
		public static bool Hit27;
		public static bool Hit28;
		public static bool Hit29;
		public static bool Hit30;
		public static bool Hit31;
		public static bool Hit32;
		public static bool Hit33;
		public static bool Hit34;
		public static bool Hit35;
		public static bool Hit36;
		public static bool Hit37;
		public static bool Hit38;
		public static bool Hit39;
		public static bool Hit40;
		public static bool Hit41;
		public static bool Hit42;
		public static bool Hit43;
		public static bool Hit44;
		public static bool Hit45;
		public static bool Hit46;
		public static bool Hit47;
		public static bool Hit48;
	}
public static ParameterSource Create(string data, VirtualMachine vm, CommonVariable? target = null,
		VmTypeInfo? expectedType = null) {
		var parameterSource = new ParameterSource();
		parameterSource.IsCommaSeparator = data.Contains(',');
		if (data.StartsWith("const_")) {
			HitTracker.Hit0 = true;
			var @int = VmTypeInfo.Int32;
			var text = data;
			parameterSource.LiteralValue = ParameterValue.Create(vm, @int, text.Substring(6, text.Length - 6));
			parameterSource.IsConstant = true;
			return parameterSource;
		}

		var flag = (parameterSource.HasLeadingPercent = data.StartsWith('%'));
		var num = data.IndexOf('%', flag ? 1 : 0);
		string content;
		string? prefixInputParamRaw = null;
		if (num != -1) {
			HitTracker.Hit1 = true;
			var text2 = data[..num];
			var num2 = num + 1;
			content = data.Substring(num2, data.Length - num2);
			if (ulong.TryParse(text2, out var result)) {
			HitTracker.Hit2 = true;
				var nullableElement = vm.GetNullableElement<VmElement>(result);
				if (nullableElement is Parameter dynamicObjectReference) {
			HitTracker.Hit3 = true;
					parameterSource.DynamicObjectReference = dynamicObjectReference;
					parameterSource.DynamicParameterName = content;
				} else if (nullableElement is ParameterHolder prefixHolder) {
			HitTracker.Hit4 = true;
					parameterSource.PrefixHolder = prefixHolder;
				} else {
			HitTracker.Hit5 = true;
					parameterSource.PrefixString = text2;
				}
			} else if (text2.Contains("H") && HierarchyGuid.TryParse(text2, vm, out var result2)) {
			HitTracker.Hit6 = true;
				parameterSource.PrefixHierarchy = result2;
			} else {
			HitTracker.Hit7 = true;
				parameterSource.PrefixString = text2;
			}
		} else if (flag) {
			HitTracker.Hit8 = true;
			content = data[1..];
		} else {
			HitTracker.Hit9 = true;
			content = data;
		}

		var holder = parameterSource.PrefixHolder;
		if (parameterSource.PrefixHierarchy != null) {
			HitTracker.Hit10 = true;
			var elements = parameterSource.PrefixHierarchy.Elements;
			holder = elements[elements.Count - 1].Element as ParameterHolder;
		}

		if (holder != null && content.Contains("_message_")) {
			HitTracker.Hit11 = true;
			var span = content.AsSpan();
			var percentIdx = span.IndexOf('%');
			var messageNameSpan = percentIdx >= 0 ? span.Slice(percentIdx + 1) : span;

			var events = EventAccessibilityUtility.GetAccessibleEvents(holder, vm);
			foreach (var e in events) {
				if (e.MessagesInfo != null) {
			HitTracker.Hit12 = true;
					foreach (var m in e.MessagesInfo) {
						if (m.Name.AsSpan().SequenceEqual(messageNameSpan)) {
			HitTracker.Hit13 = true;
							parameterSource.MessageReference = m;
							return parameterSource;
						}
					}
				}
			}
		} else if (content.Contains("_inputparam_")) {
			HitTracker.Hit14 = true;
			var array = content.Split(["_inputparam_"], StringSplitOptions.None);
			if (array.Length > 1) {
			HitTracker.Hit15 = true;
				GraphParamInfo? evtRef = null;
				Graph? foundGraph = null;

				foreach (var graph in vm.GetElementsByType<Graph>()) {
					if (graph.InputParamsInfo != null) {
			HitTracker.Hit16 = true;
						foreach (var p in graph.InputParamsInfo) {
							if (p.Name == content || p.Name == array[1]) {
			HitTracker.Hit17 = true;
								evtRef = p;
								foundGraph = graph;
								goto FoundGlobalParam;
							}
						}
					}
				}

			FoundGlobalParam:
				if (evtRef != null && evtRef.Value.Name != null) {
			HitTracker.Hit18 = true;
					parameterSource.InputParamReference = evtRef;
					parameterSource.InputParamGraphOwner = new(foundGraph!);
					return parameterSource;
				} 
			}
		} else if (content.Contains("_Loop_")) {
			HitTracker.Hit19 = true;
			ParseLoopVariable(ref parameterSource, content, vm);
		} else if (content.StartsWith("global_")) {
			HitTracker.Hit20 = true;
			if (content.Contains("_List_")) {
			HitTracker.Hit21 = true;
				ParseGlobalList(ref parameterSource, content); // false??
			} else {
			HitTracker.Hit22 = true;
				ParseGlobalVariable(ref parameterSource, content);
			}
		} else if (expectedType is { BaseType: VmType.BlueprintRef }) {
			if (ulong.TryParse(content, out var result4)) {
			HitTracker.Hit23 = true;
				var nullableElement2 = vm.GetNullableElement<Item, Other>(result4);
				if (!nullableElement2.HasValue) {
			HitTracker.Hit24 = true;
					Logger.Log(LogLevel.Error,
						$"BlueprintRef: Object with ID {result4} not found or is neither Item nor Other.");
				} else {
			HitTracker.Hit25 = true;
					parameterSource.BlueprintReference =
						new() { Element = nullableElement2.Value, SerializeAsGuid = false };
					parameterSource.ElementReference = nullableElement2.Value.Element;
				}
			} else {
			HitTracker.Hit26 = true;
				var gameObject = vm.GetElementsByType<GameObject>().FirstOrDefault(x => x.EngineTemplateId == content);
				switch (gameObject) {
					case Item item:
						parameterSource.BlueprintReference = new() { Element = new(item), SerializeAsGuid = true };
						parameterSource.ElementReference = item;
						break;
					case Other other:
						parameterSource.BlueprintReference = new() { Element = new(other), SerializeAsGuid = true };
						parameterSource.ElementReference = other;
						break;
				}
			}
		} else if (expectedType is { BaseType: VmType.EntityRef }) {
			if (ulong.TryParse(content, out var result4)) {
			HitTracker.Hit27 = true;
				var nullableElement2 = vm.GetNullableElement<GameObject>(result4);
				if (nullableElement2 == null) {
			HitTracker.Hit28 = true;
					Logger.Log(LogLevel.Error, $"EntityRef: Object with ID {result4} not found.");
				} else {
			HitTracker.Hit29 = true;
					parameterSource.EntityReference = new EntityRef {
						Element = nullableElement2, SerializeAsGuid = false
					};
					parameterSource.ElementReference = nullableElement2;
				}
			} else {
			HitTracker.Hit30 = true;
				var gameObject = vm.GetElementsByType<GameObject>().FirstOrDefault(x => x.EngineTemplateId == content);
				parameterSource.EntityReference = new EntityRef { Element = gameObject, SerializeAsGuid = true };
				parameterSource.ElementReference = gameObject;
			}
		} else if (ulong.TryParse(content, out var result6)) {
			HitTracker.Hit31 = true;
			var nullableElement3 = vm.GetNullableElement<VmElement>(result6);
			if (nullableElement3 is Parameter parameterReference) {
			HitTracker.Hit32 = true;
				parameterSource.ParameterReference = parameterReference;
			} else if (nullableElement3 != null) {
			HitTracker.Hit33 = true;
				parameterSource.ElementReference = nullableElement3;
			}
		}

		parameterSource.TypeInfo = expectedType ?? VmTypeInfo.Unknown;
		if (parameterSource.TypeInfo == VmTypeInfo.Unknown) {
			HitTracker.Hit34 = true;
			if (parameterSource.MessageReference != null) {
			HitTracker.Hit35 = true;
				parameterSource.TypeInfo = VmTypeHelper.GetVmTypeInfo(parameterSource.MessageReference.Value.Type, vm);
			} else if (parameterSource.InputParamReference != null) {
			HitTracker.Hit36 = true;
				parameterSource.TypeInfo =
					VmTypeHelper.GetVmTypeInfo(parameterSource.InputParamReference.Value.Type, vm);
			} else if (parameterSource.ParameterReference != null) {
			HitTracker.Hit37 = true;
				parameterSource.TypeInfo = VmTypeHelper.GetVmTypeInfo(parameterSource.ParameterReference.Type, vm);
			} else if (parameterSource.IsLoopIndex) {
			HitTracker.Hit38 = true;
				parameterSource.TypeInfo = VmTypeInfo.Int32;
			} else {
			HitTracker.Hit39 = true;
				if (parameterSource.IsLoopElement) {
			HitTracker.Hit40 = true;
					var loopActionLine = parameterSource.LoopActionLine;
					if (loopActionLine != null && loopActionLine.LoopInfo != null) {
			HitTracker.Hit41 = true;
						var name = parameterSource.LoopActionLine.LoopInfo.Name.GetVariableName();
						parameterSource.TypeInfo = VmTypeInfo.GameObject;
						goto IL_082f;
					}
				}

				if (parameterSource.GlobalListName != null) {
			HitTracker.Hit42 = true;
					parameterSource.TypeInfo = new VmTypeInfo(VmType.List) { UnderlyingType = VmTypeInfo.GameObject };
				} else if (target != null) {
			HitTracker.Hit43 = true;
					if (target.VariableParameter is Parameter parameter) {
			HitTracker.Hit44 = true;
						parameterSource.TypeInfo = VmTypeHelper.GetVmTypeInfo(parameter.Value.XmlType, vm);
					} else if (target.ContextParameter is Parameter parameter2) {
			HitTracker.Hit45 = true;
						parameterSource.TypeInfo = VmTypeHelper.GetVmTypeInfo(parameter2.Value.XmlType, vm);
					}
				}
			}
		}

		IL_082f:
		if (parameterSource.MessageReference == null && parameterSource.InputParamReference == null &&
		    parameterSource.ParameterReference == null && parameterSource.ElementReference == null &&
		    !parameterSource.IsLoopIndex && !parameterSource.IsLoopElement && parameterSource.GlobalListName == null &&
		    parameterSource.HierarchyReference == null && parameterSource.DynamicObjectReference == null) {
			HitTracker.Hit46 = true;
			if (content.Contains("_inputparam_") || content.Contains("_message_") || content.Contains("_Loop_")) {
			HitTracker.Hit47 = true;
				parameterSource.LiteralValue = new BasicValue<string>("Unknown", content);
				parameterSource.TypeInfo = VmTypeInfo.Unknown;
			} else {
			HitTracker.Hit48 = true;
			try {
				parameterSource.LiteralValue = ParameterValue.Create(vm, parameterSource.TypeInfo, content);
			} catch {
				Console.WriteLine($"CRASH!!! {data}");
			}
			}
			Logger.Log(LogLevel.Warning, $"Using literal parsing for {data}, guessed type:{parameterSource.TypeInfo} {parameterSource.LiteralValue?.XmlType}");
		}

		return parameterSource;
	}

	private static void ParseLoopVariable(ref ParameterSource source, string content, VirtualMachine vm) {
		var array = content.Split('_');
		if (array.Length < 2 || !ulong.TryParse(array[1], out var result)) {
			return;
		}

		source.LoopActionLine = vm.GetNullableElement<ActionLine>(result);
		if (content.EndsWith("_Index")) {
			source.IsLoopIndex = true;
		} else if (content.EndsWith("_Element")) {
			source.IsLoopElement = true;
			var num = content.IndexOf("_List_");
			var num2 = content.LastIndexOf("_Element");
			if (num != -1 && num2 != -1) {
				source.LoopListName = content.Substring(num + 6, num2 - (num + 6));
			}
		}
	}

	private static void ParseGlobalList(ref ParameterSource source, string content) {
		var num = content.LastIndexOf('_');
		if (num != -1) {
			source.GlobalListName = content.Substring(0, num);
			var num2 = num + 1;
			if (ulong.TryParse(content.Substring(num2, content.Length - num2), out var result)) {
				source.GlobalListTargetId = result;
			}
		}
	}

	private static void ParseGlobalVariable(ref ParameterSource source, string content) {
		source.GlobalListName = content;
	}

	public string? GetVariableName() {
		if (ParameterReference != null) {
			return ParameterReference.Name;
		}

		if (ElementReference is INamedElement namedElement) {
			return namedElement.Name;
		} else if (DynamicParameterName != null) {
			return DynamicParameterName;
		}

		if (GlobalListName != null) {
			return GlobalListName;
		}

		if (MessageReference != null) {
			return MessageReference.Value.Name;
		}

		if (InputParamReference != null) {
			return InputParamReference.Value.Name;
		}

		if (LiteralValue != null) {
			var text = LiteralValue.Serialize();
			return IsConstant ? ("const_" + text) : text;
		}

		return null;
	}

	public string Write() {
		if (DynamicObjectReference != null && DynamicParameterName != null) {
			return $"{DynamicObjectReference.Id}%{DynamicParameterName}";
		}

		string text = null;
		if (PrefixHolder != null) {
			text = PrefixHolder.Id.ToString();
		} else if (PrefixHierarchy != null) {
			text = PrefixHierarchy.Write();
		} else if (PrefixInputParamReference != null) {
			text = PrefixInputParamReference.Value.Name;
		} else if (PrefixString != null) {
			text = PrefixString;
		}

		if (LiteralValue != null) {
			var text2 = LiteralValue.Serialize();
			if (IsCommaSeparator) {
				text2 = text2.Replace('.', ',');
			}

			if (string.Equals(text2, "none", StringComparison.OrdinalIgnoreCase)) {
				if (text != null) {
					return text + "%" + text2;
				}

				if (!HasLeadingPercent) {
					return text2;
				}

				return "%" + text2;
			}

			text2 = (IsConstant ? ("const_" + text2) : (text2 ?? ""));
			if (text != null) {
				return text + "%" + text2;
			}

			if (!IsConstant) {
				if (!HasLeadingPercent) {
					return text2;
				}

				return "%" + text2;
			}

			return text2;
		}

		string text3 = null;
		if (IsLoopIndex) {
			text3 = $"local_{LoopActionLine.Id}_Loop_Index";
		} else if (IsLoopElement) {
			var value = LoopActionLine.LoopInfo?.Name.GetVariableName() ?? LoopListName;
			text3 = $"local_{LoopActionLine.Id}_Loop_List_{value}_Element";
		} else if (GlobalListName != null) {
			text3 = (GlobalListTargetId.HasValue ? $"{GlobalListName}_{GlobalListTargetId}" : GlobalListName);
		} else if (MessageReference != null) {
			text3 = MessageReference.Value.Name;
		} else if (InputParamReference != null) {
			text3 = InputParamReference.Value.Name;
		} else if (ParameterReference != null) {
			text3 = ParameterReference.Id.ToString();
			if (text == null) {
				text = ParameterReference.Parent.Id.ToString();
			}
		} else if (BlueprintReference != null) {
			text3 = ((!BlueprintReference.SerializeAsGuid)
				? BlueprintReference.Element.Element?.Id.ToString()
				: (BlueprintReference.Element.Element as GameObject)?.EngineTemplateId);
		} else if (EntityReference != null) {
			text3 = ((!EntityReference.SerializeAsGuid)
				? EntityReference.Element?.Id.ToString()
				: EntityReference.Element?.EngineTemplateId);
		} else if (ElementReference != null) {
			text3 = ElementReference.Id.ToString();
		} else if (HierarchyReference != null) {
			text3 = HierarchyReference.Write();
		}

		if (text != null) {
			return text + "%" + text3;
		}

		string text4;
		if (!HasLeadingPercent) {
			text4 = text3;
			if (text4 == null) {
				return "";
			}
		} else {
			text4 = "%" + text3;
		}

		return text4;
	}

	public override string ToString() => Write();
}