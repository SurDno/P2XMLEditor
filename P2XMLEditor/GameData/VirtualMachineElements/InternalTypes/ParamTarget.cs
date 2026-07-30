using System;
using System.Linq;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.GameData.VirtualMachineElements.Placeholders;
using P2XMLEditor.Helper;
using P2XMLEditor.Logging;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes;

public enum ParamTargetKind { Empty, Parameter, ComponentParam }

public readonly struct ParamTarget {
	public ParamTargetKind Kind { get; init; }

	public ParameterHolder? ContextHolder { get; init; }
	public HierarchyGuid? ContextHierarchy { get; init; }
	public VmEither<Parameter, ParameterPlaceholder>? Parameter { get; init; }
	public string? ComponentParamName { get; init; }

	public bool HasLeadingPercent { get; init; }

	public static ParamTarget Empty(bool hasLeadingPercent = true) =>
		new() { Kind = ParamTargetKind.Empty, HasLeadingPercent = hasLeadingPercent };

	public static bool TryRead(string data, VirtualMachine vm, out ParamTarget result) {
		var leading = data.StartsWith('%');
		var body = leading ? data[1..] : data;

		string? context = null;
		var sep = body.IndexOf('%');
		if (sep != -1) {
			context = body[..sep];
			body = body[(sep + 1)..];
			if (context.Length == 0) context = null;
		}

		if (body.Length == 0) {
			result = Empty(leading);
			return true;
		}

		// "Component.Param" — tested before the id lookup; no other form contains a '.'.
		if (IsComponentParamName(body)) {
			result = new() { Kind = ParamTargetKind.ComponentParam, ComponentParamName = body,
							 HasLeadingPercent = leading };
			return true;
		}

		if (ulong.TryParse(body, out var id)) {
			var element = vm.GetNullableElement(id);
			VmEither<Parameter, ParameterPlaceholder>? param = element switch {
				ParameterPlaceholder ph => new(ph),
				Parameter p => new(p),
				null => new(vm.Register(new ParameterPlaceholder(id))),
				_ => null 
			};

			if (param != null) {
				result = new() { Kind = ParamTargetKind.Parameter, Parameter = param, HasLeadingPercent = leading };
				if (context == null) return true;

				if (ulong.TryParse(context, out var contextId) &&
				    vm.GetNullableElement(contextId) is ParameterHolder holder)
					result = result with { ContextHolder = holder };
				else if (HierarchyGuid.TryParse(context, vm, out var contextHierarchy))
					result = result with { ContextHierarchy = contextHierarchy };
				else
					Logger.Log(LogLevel.Warning, $"TargetParam '{data}' has an unresolved context '{context}'.");

				return true;
			}
		}

		result = default;
		return false;
	}

	private static bool IsComponentParamName(string s) {
		var dot = s.IndexOf('.');
		if (dot <= 0 || dot == s.Length - 1) return false;
		return s.All(c => char.IsAsciiLetterOrDigit(c) || c == '_' || c == '.');
	}

	public string Write() {
		var context = ContextHolder != null ? ContextHolder.Id.ToString() : ContextHierarchy?.Write();

		var value = Kind switch {
			ParamTargetKind.Empty          => "",
			ParamTargetKind.Parameter      => Parameter!.Value.Id.ToString(),
			ParamTargetKind.ComponentParam => ComponentParamName!,
			_ => throw new InvalidOperationException($"Cannot write an uninitialised {nameof(ParamTarget)}.")
		};

		if (context != null) return $"{context}%{value}";
		return HasLeadingPercent ? "%" + value : value;
	}

	public override string ToString() =>  Write();
}
