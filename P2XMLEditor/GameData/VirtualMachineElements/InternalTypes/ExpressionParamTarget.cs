using System;
using System.Linq;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.GameData.VirtualMachineElements.Placeholders;
using P2XMLEditor.Helper;
using P2XMLEditor.Logging;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes;

public enum ExpressionParamKind {
	Param,
	Message,
	InputParam,
	ObjectLiteral,
	Unresolved
}

public readonly struct ExpressionParamTarget {
	public ExpressionParamKind Kind { get; init; }

	public ParamTarget? Param { get; init; }
	public Message? Message { get; init; }
	public InputParameter? InputParam { get; init; }
	public VmElement? ObjectLiteral { get; init; }
	public string? UnresolvedRawString{ get; init; }
	public HierarchyGuid? LiteralHierarchy { get; init; }
	public bool ByEngineGuid { get; init; }

	public VmTypeInfo? ValueType { get; init; }

	public bool IsLiteral => Kind == ExpressionParamKind.ObjectLiteral;
	public bool HasLeadingPercent { get; init; }

	public static ExpressionParamTarget Read(string data, VirtualMachine vm, VmElement? scope = null) {
		if (ParamTarget.TryRead(data, vm, out var param))
			return new() {
				Kind = ExpressionParamKind.Param,
				Param = param,
				ValueType = param.Parameter?.Element is Parameter parameter and not ParameterPlaceholder 
					? VmTypeHelper.GetVmTypeInfo(parameter.Type, vm) : null
			};

		var leading = data.StartsWith('%');
		var body = leading ? data[1..] : data;

		if (body.Contains("_message_") && InternalTypes.Message.TryParse(body, vm, out var msg))
			return new() {
				Kind = ExpressionParamKind.Message,
				Message = msg,
				ValueType = VmTypeHelper.GetVmTypeInfo(msg!.Type, vm),
				HasLeadingPercent = leading
			};

		if (body.Contains("_inputparam_") && InputParameter.TryParse(body, out var ip, scope))
			return new() {
				Kind = ExpressionParamKind.InputParam,
				InputParam = ip,
				ValueType = VmTypeHelper.GetVmTypeInfo(ip!.Type, vm),
				HasLeadingPercent = leading
			};

		if (HierarchyGuid.TryParse(body, vm, out var hierarchy))
			return new() {
				Kind = ExpressionParamKind.ObjectLiteral,
				LiteralHierarchy = hierarchy,
				ValueType = VmTypeInfo.GameObject,
				HasLeadingPercent = leading
			};

		if (ulong.TryParse(body, out var id) && vm.GetNullableElement(id) is { } element)
			return new() {
				Kind = ExpressionParamKind.ObjectLiteral,
				ObjectLiteral = element,
				ValueType = VmTypeHelper.GetVmType(element.GetType()),
				HasLeadingPercent = leading
			};

		if (Guid.TryParse(body, out _)) {
			var byGuid = vm.GetElementsByType<GameObject>().FirstOrDefault(o => o.EngineTemplateId == body);
			if (byGuid != null)
				return new() {
					Kind = ExpressionParamKind.ObjectLiteral,
					ObjectLiteral = byGuid, ByEngineGuid = true,
					ValueType = VmTypeHelper.GetVmType(byGuid.GetType()),
					HasLeadingPercent = leading
				};
		}

		Logger.Log(LogLevel.Warning, $"Unresolved Expression TargetParam '{body}'.");
		return new() { Kind = ExpressionParamKind.Unresolved, UnresolvedRawString = data} ;
	}

	public string Write() {
		if (Kind == ExpressionParamKind.Param) return Param!.Value.Write();

		var value = Kind switch {
			ExpressionParamKind.Message      => Message!.Name,
			ExpressionParamKind.InputParam   => InputParam!.Name,
			ExpressionParamKind.ObjectLiteral => LiteralHierarchy != null ? LiteralHierarchy.Write()
				: ByEngineGuid ? (ObjectLiteral as GameObject)!.EngineTemplateId
				: ObjectLiteral!.Id.ToString(),
			ExpressionParamKind.Unresolved => UnresolvedRawString,
			_ => throw new InvalidOperationException($"Unhandled {nameof(ExpressionParamKind)} {Kind}")
		};

		return HasLeadingPercent ? "%" + value : value;
	}

	public override string ToString() => Write();
}