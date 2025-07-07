using System.Diagnostics.CodeAnalysis;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.Suggestions.Abstract;

namespace P2XMLEditor.Suggestions.Refactoring.Deprecated;

// TODO: fix. Currently not working as expected.
//[Refactoring("Refactor/Remove orphaned game strings")]
[SuppressMessage("ReSharper", "UnusedType.Global")]
public class RemoveUnusedStorableParameters(VirtualMachine vm) : Suggestion(vm) {
	public override void Execute() {
		foreach (var item in Vm.GetElementsByType<ParameterHolder>()) {
			if (item.StandartParams.TryGetValue("Storable.SpecialDescription", out var specialDesc))
			    Vm.RemoveElement(specialDesc);
			if (item.StandartParams.TryGetValue("Storable.StoreTag", out var storeTag))
				Vm.RemoveElement(storeTag);
			if (item.StandartParams.TryGetValue("Storable.StorableClass", out var storableClass))
				Vm.RemoveElement(storableClass);
		}
	}
}