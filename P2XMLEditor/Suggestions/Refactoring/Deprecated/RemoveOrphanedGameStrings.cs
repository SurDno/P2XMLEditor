using System.Diagnostics.CodeAnalysis;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements;

namespace P2XMLEditor.Suggestions.Refactoring.Deprecated;

// TODO: fix. Currently not working as expected.
//[Refactoring("Refactor/Remove orphaned game strings")]
[SuppressMessage("ReSharper", "UnusedType.Global")]
public class RemoveOrphanedGameStrings(VirtualMachine vm) : Suggestion(vm) {
	public override void Execute() {
		var gameStrings = Vm.GetElementsByType<GameString>();
		var orphanedGameStrings = gameStrings.Where(item => item.IsOrphaned()).ToList();
		foreach (var item in orphanedGameStrings)
			Vm.RemoveElement(item);
	}
}