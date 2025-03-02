using P2XMLEditor.Abstract;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.GameData.VirtualMachineElements.Enums;

namespace P2XMLEditor.Refactoring;

public class SimplifyTrueComparisons(VirtualMachine vm) : RefactoringSuggestion(vm) {
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