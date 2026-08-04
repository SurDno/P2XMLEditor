using System;
using System.Linq;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.GameData.VirtualMachineElements.Enums;
using P2XMLEditor.GameData.VirtualMachineElements.Interfaces;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes;

namespace P2XMLEditor.Helper;

public static class PreviewHelper {

	public static string Preview(VmElement vmElement) => vmElement switch {
		PartCondition pc => Preview(pc),
		Condition cond => Preview(cond),
		Expression expr => Preview(expr),
		_ => throw new ArgumentOutOfRangeException()
	};

	public static string Preview(Condition cond) {
		return
			$"({string.Join(cond.Operation switch { ConditionOperation.And => " && ", ConditionOperation.Or => " || ",
				ConditionOperation.Xor => " ^ ", ConditionOperation.Root => "" }, cond.Predicates.Select(p =>
				p.Element switch { Condition subcond => Preview(subcond), PartCondition pc => Preview(pc) }))})";
	}

	public static string Preview(PartCondition pc) {
		return pc.ConditionType switch {
			ConditionType.ConstFalse => "false",
			ConditionType.ConstTrue => "true",
			ConditionType.ValueLess => $"{Preview(pc.FirstExpression!)} < {Preview(pc.SecondExpression!)}",
			ConditionType.ValueLessEqual => $"{Preview(pc.FirstExpression!)} <= {Preview(pc.SecondExpression!)}",
			ConditionType.ValueLarger => $"{Preview(pc.FirstExpression!)} > {Preview(pc.SecondExpression!)}",
			ConditionType.ValueLargerEqual => $"{Preview(pc.FirstExpression!)} >= {Preview(pc.SecondExpression!)}",
			ConditionType.ValueEqual => $"{Preview(pc.FirstExpression!)} == {Preview(pc.SecondExpression!)}",
			ConditionType.ValueNotEqual => $"{Preview(pc.FirstExpression!)} != {Preview(pc.SecondExpression!)}",
			ConditionType.ValueExpression => $"{Preview(pc.FirstExpression!)}",
			_ => throw new NotImplementedException()
		};
	}

	public static string Preview(Parameter p) {
		return p.Value switch {
			RefValue<GameString> textRef => textRef.TypedValue?.GetText("English") is { Length: > 0 } en ? en : 
											textRef.TypedValue?.GetText("Russian") is { Length: > 0 } ru ? ru : 
											textRef.TypedValue?.Id.ToString() ?? p.SerializedValue,
			RefValue<VmElement> refVal => (refVal.TypedValue as INamedElement)?.Name ?? refVal.TypedValue?.Id.ToString() ?? p.SerializedValue,
			BasicValue<bool> b => b.TypedValue ? "true" : "false",
			_ => p.SerializedValue
		};
	}

	public static string Preview(Expression? expression) {
		return expression?.ExpressionType switch {
			null => "<none>",
			ExpressionType.Const => Preview(expression.Const!),
			ExpressionType.Function =>
				$"{expression.Function!.Name}({string.Join(',', GetPreviewParamStrings(expression.Function!))})",
			ExpressionType.Param => Preview(expression.TargetObject) + " " + Preview(expression.TargetParam),
			ExpressionType.Complex => "Not supported expression",
			_ => throw new ArgumentOutOfRangeException()
		};
	}

	private static System.Collections.Generic.IEnumerable<string> GetPreviewParamStrings(P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract.VmFunction function) {
		var raw = function.GetParamStrings();
		if (raw == null || raw.Count == 0) return [];

		var properties = P2XMLEditor.GameData.VirtualMachineElements.Helper.FunctionSignature.SlotProperties(function.GetType());
		if (properties.Length != raw.Count) return raw;

		var results = new System.Collections.Generic.List<string>(raw.Count);
		for (var i = 0; i < properties.Length; i++) {
			if (P2XMLEditor.GameData.VirtualMachineElements.Helper.FunctionSignature.LiveSource(properties[i], function) is { } source) {
				results.Add(source.GetVariableName() ?? source.Write());
			} else {
				results.Add(raw[i]);
			}
		}
		return results;
	}

	public static string Preview(TargetObject targetObject) {
		if (!targetObject.IsSet) return "<null>";
		var name = targetObject.Kind switch {
			TargetObjectKind.Holder => targetObject.Holder?.Name,
			TargetObjectKind.ParameterRef => targetObject.ParameterRef?.Name,
			TargetObjectKind.Message => targetObject.Message?.Name,
			TargetObjectKind.InputParam => targetObject.InputParam?.Name,
			TargetObjectKind.Loop => targetObject.Loop?.ParamId,
			_ => null
		};
		var result = name ?? targetObject.Write();
		return targetObject.HasLeadingPercent ? "%" + result : result;
	}

	public static string Preview(ExpressionParamTarget? targetParam) {
		if (targetParam == null) return "<null>";
		var tp = targetParam.Value;
		var name = tp.Kind switch {
			ExpressionParamKind.Param => tp.Param?.Parameter?.Element is Parameter p ? p.Name : null,
			ExpressionParamKind.Message => tp.Message?.Name,
			ExpressionParamKind.InputParam => tp.InputParam?.Name,
			ExpressionParamKind.ObjectLiteral => tp.ObjectLiteral is INamedElement ne ? ne.Name : null,
			_ => null
		};
		var result = name ?? tp.Write();
		return tp.HasLeadingPercent ? "%" + result : result;
	}

}
