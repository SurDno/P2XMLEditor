using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualBasic.Logging;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.Helper;
using P2XMLEditor.Suggestions.Abstract;
using P2XMLEditor.Suggestions.Attributes;
using Action = P2XMLEditor.GameData.VirtualMachineElements.Action;

namespace P2XMLEditor.Suggestions.Cleanup;

// TODO: completely refactor once we start storing Action function references and Parameter types normally. 
[Cleanup("References/Delete storables not appearing in combinations")]
[SuppressMessage("ReSharper", "UnusedType.Global")]
public class DeleteStorablesNotAppearingInCombinations(VirtualMachine vm) : Suggestion(vm) {
	public override void Execute() {
		var all = Vm.GetElementsByType<Item>().Cast<GameObject>().Concat(Vm.GetElementsByType<Other>()).ToList();
		var combos = all.Where(i => i.StandartParams.ContainsKey(CombinationHelper.CombinationKey)).ToList();
		var items = all.Where(i => i.StandartParams.ContainsKey(CombinationHelper.StorableKey)).Except(combos).ToList();

		var parameters = Vm.GetElementsByType<Parameter>().ToList();
		var actions = Vm.GetElementsByType<Action>().ToList();
		
		foreach (var storable in items.Where(item => CombinationHelper.GetCombinationsWithItem(Vm, item).Count == 0)) {

			var referencedInParameters = false;
			foreach (var parameter in parameters) {
				if (!parameter.Type.Contains("IBlueprintRef")) continue;
				if (parameter.Value.Contains(storable.EngineTemplateId!)) {
					referencedInParameters = true;
					break;
				}
			}
			if (referencedInParameters) continue;
			var referencedInActions = false;
			foreach (var action in actions) {
				if (!action.TargetFuncName.Contains("PickUpByTemplate") && 
				    !action.TargetFuncName.Contains("PickUpToInentoryByTemplate") && 
				    !action.TargetFuncName.Contains("RemoveThingByTemplate")) continue;
				if (action.SourceParams == null) continue;
				foreach (var param in action.SourceParams.Where(p => p.Contains(storable.EngineTemplateId!))) {
					referencedInActions = true;
					break;
				}
			}
			if (referencedInActions) continue;
			
			Logger.LogInfo($"Deleting unused storable {storable.Name}.");
			Vm.RemoveElement(storable);
		}
	}
}
