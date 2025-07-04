
using System.Diagnostics.CodeAnalysis;
using P2XMLEditor.Abstract;
using P2XMLEditor.Attributes;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.GameData.VirtualMachineElements.Enums;

namespace P2XMLEditor.Refactoring;

// TODO: fix. Currently not working as expected.
//[Refactoring("Refactor/Simplify true comparisons")]
[SuppressMessage("ReSharper", "UnusedType.Global")]
public class SimplifyTrueComparisons(VirtualMachine vm) : Suggestion(vm) {
	public override void Execute() {
		var pcs = _vm.GetElementsByType<PartCondition>();

		foreach (var pc in pcs) {
			if (pc is { ConditionType: ConditionType.ValueEqual, SecondExpression.ExpressionType: ExpressionType.Const }
			    && pc.SecondExpression.Const!.Value == "True") {
					_vm.RemoveElement(pc.SecondExpression.Const!);
					_vm.RemoveElement(pc.SecondExpression);
					pc.SecondExpression = null;
					pc.ConditionType = ConditionType.ValueExpression;
			}
		}
	}
}