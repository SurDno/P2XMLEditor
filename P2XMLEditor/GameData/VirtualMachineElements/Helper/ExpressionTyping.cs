using P2XMLEditor.Core;
using P2XMLEditor.GameData;
using P2XMLEditor.GameData.Enums;
using P2XMLEditor.GameData.VirtualMachineElements.Enums;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes;
using P2XMLEditor.Helper;
using ExprKind = P2XMLEditor.GameData.VirtualMachineElements.Enums.ExpressionType;

namespace P2XMLEditor.GameData.VirtualMachineElements.Helper;

/// <summary>
/// What an expression evaluates to, and what the place it sits in expects of it.
///
/// An expression is an operand, so unlike an action it always has a value and a type. Both
/// sides of a comparison have to agree, which gives the editor its constraint: whichever side
/// is already filled in types the other. Nothing types the first side of an empty condition,
/// and that is a real answer rather than a gap — until something is chosen, anything is legal.
/// </summary>
public static class ExpressionTyping {
	/// <summary>What this expression evaluates to, or null when it cannot be told.</summary>
	public static VmTypeInfo? TypeOf(Expression? expression, VirtualMachine vm) {
		if (expression == null) return null;

		switch (expression.ExpressionType) {
			case ExprKind.Const:
				return expression.Const is { } constant ? VmTypeHelper.GetVmTypeInfo(constant.Type, vm) : null;

			case ExprKind.Param:
				// An object written in directly is not typed by what it happens to be. The
				// engine stores a bare id or a hierarchy path and reads it back with the type
				// the other side wants: the same character id answers as ICharacterRef, as
				// IBlueprintRef when compared against a template, and as IEntity when compared
				// against a region. Reporting the element's own class here would have the
				// editor refuse comparisons the game itself ships — 64 of them across the two
				// corpora, among them every "Get object template(X) == <character>".
				if (expression.TargetParam is { IsLiteral: true }) return null;

				// Everything else really is declared: ExpressionParamTarget works the type out
				// while reading, off the parameter's own declaration or the message or input
				// param it names.
				return expression.TargetParam?.ValueType;

			case ExprKind.Function:
				var name = expression.Function?.Name;
				if (string.IsNullOrEmpty(name)) return null;
				var signature = FunctionSignature.Of(name, vm);
				return signature == null || signature.IsVoid ? null : signature.ReturnTypeInfo;

			case ExprKind.Complex:
				// A formula's operands are all one type, so the first that answers types it.
				foreach (var child in expression.FormulaChilds ?? [])
					if (TypeOf(child, vm) is { } childType)
						return childType;
				return null;

			default:
				return null;
		}
	}

	/// <summary>
	/// What the expression on one side of a comparison has to produce, or null when nothing
	/// constrains it yet.
	///
	/// Comparisons type each side by the other. Inversion does not enter into it — it negates
	/// the result of the comparison, not the type of an operand.
	///
	/// ValueExpression is the odd one out and constrains nothing. It reads as "the condition's
	/// value is this expression" rather than "is this expression true", and the data bears that
	/// out: of the 1295 vanilla ValueExpression conditions, 1010 have a number as their first
	/// operand — branch weights, dialogue priorities and the like — and only 285 a boolean.
	/// Demanding a boolean here would hide the majority of them.
	/// </summary>
	public static VmTypeInfo? ExpectedFor(PartCondition? condition, bool firstSide, VirtualMachine vm) {
		if (condition == null) return null;

		switch (condition.ConditionType) {
			case ConditionType.ConstTrue:
			case ConditionType.ConstFalse:
			case ConditionType.ValueExpression:
				return null;

			default:
				return TypeOf(firstSide ? condition.SecondExpression : condition.FirstExpression, vm);
		}
	}

	/// <summary>
	/// Whether a function may stand as an expression at all.
	///
	/// A void function has no value to compare, so it can never be one — true whatever the slot
	/// expects, which is why this holds even when the expected type is unknown. Unknown is not
	/// void: <see cref="FunctionSignature.IsVoid"/> lumps the two together, which is right for
	/// asking "does this return something useful" but wrong here, where an unreadable signature
	/// should not silently hide a function that is fine.
	/// </summary>
	public static bool CanBeExpression(FunctionSignature? signature) =>
		signature != null && signature.ReturnType != VmType.Void;

	/// <summary>
	/// Whether a function's result fits where the expression is being used. An unknown expected
	/// type admits anything that is not void — the caller has nothing to check against.
	/// </summary>
	public static bool Fits(FunctionSignature? signature, VmTypeInfo? expected, VirtualMachine vm) {
		if (!CanBeExpression(signature)) return false;
		if (expected == null || expected.BaseType == VmType.Unknown) return true;
		if (signature!.ReturnType == VmType.Unknown) return true;
		return VmTypeCompatibility.Matches(expected, signature.ReturnTypeInfo);
	}
}
