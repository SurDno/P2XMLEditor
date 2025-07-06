using System.Diagnostics.CodeAnalysis;
using P2XMLEditor.Abstract;
using P2XMLEditor.Attributes;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;

namespace P2XMLEditor.Refactoring;

[Refactoring("Refactor/Parameters/Remove Object.Enabled from storables")]
[SuppressMessage("ReSharper", "UnusedType.Global")]
public class RemoveObjectEnabledFromStorables(VirtualMachine vm) : Suggestion(vm) {
	public override void Execute() {
		foreach (var item in Vm.GetElementsByType<ParameterHolder>()) {
			if (item.StandartParams.Any(param => param.Key.Contains("Combination"))) continue;
			if (!item.StandartParams.Any(param => param.Key.Contains("Storable"))) continue;
			if(item.StandartParams.TryGetValue("Common.ObjectEnabled", out var objEnabled))
				Vm.RemoveElement(objEnabled);
		}
	}
}