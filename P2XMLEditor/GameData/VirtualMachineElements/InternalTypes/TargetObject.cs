using System;
using System.Linq;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.Helper;
using P2XMLEditor.Logging;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes;

public enum TargetObjectKind { Holder, ParameterRef, Hierarchy, Loop, InputParam, Message }

public readonly struct TargetObject {
	public TargetObjectKind Kind { get; init; }

	public ParameterHolder? Holder { get; init; }
	public Parameter? ParameterRef { get; init; } // IObjRef
	public HierarchyGuid? Hierarchy { get; init; }
	public LoopParameter? Loop { get; init; }
	public InputParameter? InputParam { get; init; }
	public Message? Message { get; init; }

	public Event? MessageOwner { get; init; }

	public bool IsSelf { get; init; }

	public bool ByEngineGuid { get; init; }

	public bool HasLeadingPercent { get; init; }

	public static TargetObject Read(string data, VirtualMachine vm, VmElement? scope = null) {
		var leading = data.StartsWith('%');
		var body = leading ? data[1..] : data;

		var isSelf = false;
		var sep = body.IndexOf('%');
		if (sep != -1) {
			var left = body[..sep];
			var right = body[(sep + 1)..];
			if (left == right) { isSelf = true; body = left; }
			else {
				Logger.Log(LogLevel.Warning, $"TargetObject '{data}' has a distinct context part; not seen in vanilla data.");
				body = right;
			}
		}

		if (HierarchyGuid.TryParse(body, vm, out var hierarchy))
			return new() { Kind = TargetObjectKind.Hierarchy, Hierarchy = hierarchy,
						   IsSelf = isSelf, HasLeadingPercent = leading };

		if (body.Contains("_message_") && vm.TryResolveMessage(body, out var msg))
			return new() { Kind = TargetObjectKind.Message, Message = msg, HasLeadingPercent = leading };

		if (body.Contains("_inputparam_") && InputParameter.TryParse(body, out var ip, scope))
			return new() { Kind = TargetObjectKind.InputParam, InputParam = ip, HasLeadingPercent = leading };

		if (body.Contains("_Loop_") && LoopParameter.TryParse(body, vm, out var loop))
			return new() { Kind = TargetObjectKind.Loop, Loop = loop, HasLeadingPercent = leading };

		if (ulong.TryParse(body, out var id))
			switch (vm.GetNullableElement(id)) {
				case Parameter p:
					return new() { Kind = TargetObjectKind.ParameterRef, ParameterRef = p,
								   IsSelf = isSelf, HasLeadingPercent = leading };
				case ParameterHolder h:
					return new() { Kind = TargetObjectKind.Holder, Holder = h,
								   IsSelf = isSelf, HasLeadingPercent = leading };
			}

		if (Guid.TryParse(body, out _)) {
			var byGuid = vm.GetElementsByType<GameObject>().FirstOrDefault(o => o.EngineTemplateId == body);
			if (byGuid != null)
				return new() { Kind = TargetObjectKind.Holder, Holder = byGuid, IsSelf = isSelf,
							   ByEngineGuid = true, HasLeadingPercent = leading };
		}

		Logger.Log(LogLevel.Error, $"Unresolved TargetObject '{data}'.");
		return default;
	}

	public string Write() {
		var value = Kind switch {
			TargetObjectKind.Holder => ByEngineGuid
				? (Holder as GameObject)?.EngineTemplateId ?? Holder!.Id.ToString()
				: Holder!.Id.ToString(),
			TargetObjectKind.ParameterRef => ParameterRef!.Id.ToString(),
			TargetObjectKind.Hierarchy    => Hierarchy!.Write(),
			TargetObjectKind.Loop         => Loop!.ParamId,
			TargetObjectKind.InputParam   => InputParam!.Name,
			TargetObjectKind.Message      => Message!.Name,
			_ => throw new InvalidOperationException($"Cannot write an uninitialised {nameof(TargetObject)}.")
		};

		if (IsSelf) value = $"{value}%{value}";
		return HasLeadingPercent ? "%" + value : value;
	}

	public ParameterHolder? ResolvedHolder => Kind switch {
		TargetObjectKind.Holder    => Holder,
		TargetObjectKind.Hierarchy => Hierarchy!.Elements[^1].Element as ParameterHolder, // MIGHT be invalid if its ScenePlaceholder - that one is just VmElement.
		_ => null
	};

	public override string ToString() => Write();
}
