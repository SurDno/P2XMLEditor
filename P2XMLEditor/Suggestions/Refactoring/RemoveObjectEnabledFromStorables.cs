using System.Diagnostics.CodeAnalysis;
using System.Linq;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.Logging;

namespace P2XMLEditor.Suggestions.Refactoring;

[Refactoring("Refactor/Parameters/Remove Object.Enabled from storables"),
 SuppressMessage("ReSharper", "UnusedType.Global")]
public class RemoveObjectEnabledFromStorables(VirtualMachine vm) : Suggestion(vm) {
	public override void Execute() {
		int removedCount = 0;
		foreach (var item in Vm.GetElementsByType<ParameterHolder>()) {
			if (item.StandartParams.Any(param => param.Key.Contains("Combination"))) continue;
			if (!item.StandartParams.Any(param => param.Key.Contains("Storable"))) continue;
			if(item.StandartParams.TryGetValue("Common.ObjectEnabled", out var objEnabled)) {
				Vm.RemoveElement(objEnabled);
				Logger.Log(LogLevel.Info, $"Removed ObjectEnabled parameter from Storable ParameterHolder '{item.Name}'");
				removedCount++;
			}
		}
		
		Logger.Log(LogLevel.Info, $"Completed: Removed ObjectEnabled from {removedCount} storables.");
	}
}
