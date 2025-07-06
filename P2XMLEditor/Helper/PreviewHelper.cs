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

	public static string Preview(Expression? expression) {
		return expression?.ExpressionType switch {
			null => "<none>",
			ExpressionType.Const => expression.Const!.Value,
			ExpressionType.Function =>
				$"{expression.Function!.Name}({string.Join(',', expression.Function!.GetParamStrings() ?? [])})",
			ExpressionType.Param => Preview(expression.TargetObject) + " " + Preview(expression.TargetParam),
			ExpressionType.Complex => "Not supported expression",
			_ => throw new ArgumentOutOfRangeException()
		};
	}

	public static string Preview(CommonVariable? commonVariable) {
		return $"{Preview(commonVariable.VariableParameter)}%{Preview(commonVariable.ContextParameter)}";
	}

	public static string Preview(ICommonVariableParameter? commonVariableParameter) {
		switch (commonVariableParameter) {
			case null:
				return string.Empty;
			case ParamByName paramByName:
				return paramByName.Id;
			case ParameterHolder ph:
				return ph.Name;
			case Parameter param:
				return param.Name;
			default:
				return commonVariableParameter.Id;
		}
	}
}