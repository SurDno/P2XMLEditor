using System.Diagnostics.CodeAnalysis;
using System.Linq;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.Logging;

namespace P2XMLEditor.Suggestions.Refactoring;

[Refactoring("Refactor/Parameters/Remove unused components from combinations"),
 SuppressMessage("ReSharper", "UnusedType.Global")]
public class RemoveUnusedComponentsFromCombinations(VirtualMachine vm) : Suggestion(vm) {
	public override void Execute() {
		int paramRemoved = 0;
		int compRemoved = 0;
		foreach (var item in Vm.GetElementsByType<ParameterHolder>()) {
			if (!item.StandartParams.Any(param => param.Key.Contains("Combination"))) continue;
			foreach (var param in item.StandartParams.Where(param => !param.Key.Contains("Combination")).ToList()) {
				Vm.RemoveElement(param.Value);
				Logger.Log(LogLevel.Info, $"Removed unused parameter '{param.Key}' from combination '{item.Name}'");
				paramRemoved++;
			}
			foreach (var comp in item.FunctionalComponents.Where(c => c.Name != "Combination" ).ToList()) {
				Vm.RemoveElement(comp);
				Logger.Log(LogLevel.Info, $"Removed unused component '{comp.Name}' from combination '{item.Name}'");
				compRemoved++;
			}
		}
		Logger.Log(LogLevel.Info, $"Completed: Removed {paramRemoved} parameters and {compRemoved} components from combinations.");
	}
}
