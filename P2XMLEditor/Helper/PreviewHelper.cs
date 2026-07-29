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
				$"{expression.Function!.Name}({string.Join(',', expression.Function!.GetParamStrings() ?? [])})",
			ExpressionType.Param => Preview(expression.TargetObject) + " " + Preview(expression.TargetParam),
			ExpressionType.Complex => "Not supported expression",
			_ => throw new ArgumentOutOfRangeException()
		};
	}

	public static string Preview(TargetObject targetObject) => targetObject.Write();
	public static string Preview(ExpressionParamTarget? targetParam) => targetParam?.Write() ?? "<null>";

}
