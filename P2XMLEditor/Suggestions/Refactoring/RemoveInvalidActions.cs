using System.Collections.Generic;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements;

namespace P2XMLEditor.Suggestions.Refactoring;

[Refactoring("Refactor/Actions/Remove Invalid Actions")]
public class RemoveInvalidActions(VirtualMachine vm) : Suggestion(vm) {
	public override void Execute() {
		var elementsByType = vm.GetElementsByType<Action>();
		new List<Action>();
		foreach (var item in elementsByType) {
			_ = item;
		}
	}
}
