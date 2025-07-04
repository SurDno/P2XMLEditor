using System.Diagnostics.CodeAnalysis;
using P2XMLEditor.Abstract;
using P2XMLEditor.Attributes;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements;

namespace P2XMLEditor.Refactoring;

[Refactoring("Refactor/Parameters/Remove unused parameters from combinations")]
[SuppressMessage("ReSharper", "UnusedType.Global")]
public class RemoveUnusedParametersFromCombinations(VirtualMachine vm) : Suggestion(vm) {
	public override void Execute() {
		foreach (var item in _vm.GetElementsByType<Item>()) {
			if (!item.StandartParams.Any(param => param.Key.Contains("Combination"))) continue;
			foreach (var param in item.StandartParams.Where(param => !param.Key.Contains("Combination")).ToList())
				_vm.RemoveElement(param.Value);
			foreach (var comp in item.FunctionalComponents.Where(c => c.Name != "Combination" && c.Events.Count == 0).ToList())
				_vm.RemoveElement(comp);
		}
	}
}