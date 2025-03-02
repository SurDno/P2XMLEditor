using P2XMLEditor.Abstract;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.GameData.VirtualMachineElements.Enums;

namespace P2XMLEditor.Refactoring;

public class RemoveOrphanedGameStrings(VirtualMachine vm) : RefactoringSuggestion(vm) {
	public override void Execute() {
		var gameStrings = _vm.GetElementsByType<GameString>();
		var orphanedGameStrings = gameStrings.Where(item => item.IsOrphaned()).ToList();
		foreach (var item in orphanedGameStrings)
			_vm.RemoveElement(item);
	}
}