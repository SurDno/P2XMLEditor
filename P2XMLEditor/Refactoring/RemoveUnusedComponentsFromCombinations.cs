using System.Diagnostics.CodeAnalysis;
using P2XMLEditor.Abstract;
using P2XMLEditor.Attributes;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;

namespace P2XMLEditor.Refactoring;

[Refactoring("Refactor/Parameters/Remove unused components from combinations")]
[SuppressMessage("ReSharper", "UnusedType.Global")]
public class RemoveUnusedComponentsFromCombinations(VirtualMachine vm) : Suggestion(vm) {
	public override void Execute() {
		foreach (var item in Vm.GetElementsByType<ParameterHolder>()) {
			if (!item.StandartParams.Any(param => param.Key.Contains("Combination"))) continue;
			foreach (var param in item.StandartParams.Where(param => !param.Key.Contains("Combination")).ToList())
				Vm.RemoveElement(param.Value);
			foreach (var comp in item.FunctionalComponents.Where(c => c.Name != "Combination" && c.Events.Count == 0).ToList())
				Vm.RemoveElement(comp);
		}
	}
}