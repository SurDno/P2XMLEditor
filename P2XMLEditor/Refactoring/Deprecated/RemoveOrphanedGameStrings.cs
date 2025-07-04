using System.Diagnostics.CodeAnalysis;
using P2XMLEditor.Abstract;
using P2XMLEditor.Attributes;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.GameData.VirtualMachineElements.Enums;

namespace P2XMLEditor.Refactoring;

// TODO: fix. Currently not working as expected.
//[Refactoring("Refactor/Remove orphaned game strings")]
[SuppressMessage("ReSharper", "UnusedType.Global")]
public class RemoveOrphanedGameStrings(VirtualMachine vm) : Suggestion(vm) {
	public override void Execute() {
		var gameStrings = _vm.GetElementsByType<GameString>();
		var orphanedGameStrings = gameStrings.Where(item => item.IsOrphaned()).ToList();
		foreach (var item in orphanedGameStrings)
			_vm.RemoveElement(item);
	}
}