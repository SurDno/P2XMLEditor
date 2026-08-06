using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions.Actions.GlobalStorageManager;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions.Actions.Storage;
using P2XMLEditor.Helper;
using P2XMLEditor.Logging;
using Action = P2XMLEditor.GameData.VirtualMachineElements.Action;

namespace P2XMLEditor.Suggestions.Cleanup;

// TODO: completely refactor once we start storing Action function references normally. 
[Cleanup("References/Game Objects/Delete unused combinations"), SuppressMessage("ReSharper", "UnusedType.Global")]
public class DeleteUnusedCombinations(VirtualMachine vm) : Suggestion(vm) {
	public override void Execute() {
		var combos = Vm.GetElementsByType<Item>().Cast<GameObject>().Concat(Vm.GetElementsByType<Other>()).ToList()
			.Where(i => i.StandartParams.ContainsKey(CombinationHelper.CombinationKey)).ToList();

		var expressions = Vm.GetElementsByType<Expression>().ToList();
		var actions = Vm.GetElementsByType<Action>().ToList();
		
		var combosToDelete = combos.Where(item => CombinationHelper.GetCombinationsWithItem(Vm, item).Count == 0).ToList();
		var stringReferences = new System.Collections.Generic.List<string>();
		var referencedComboElements = new System.Collections.Generic.HashSet<ulong>();
		
		foreach (var action in actions) {
			switch (action.Function) {
				case StoragePickUpCombinationToInentoryByTemplateFunction f1 when (f1.CombinationObject.Element?.Element.Element != null):
					referencedComboElements.Add(f1.CombinationObject.Element.Element.Element.Id);
					break;
				case GlobalStorageManagerAddItemsToStoragesLinearFunction { ContainerStatusesData: not null } f2:
					stringReferences.Add(f2.ContainerStatusesData.Write());
					break;
				case StoragePickUpCombinationFunction f3 when (f3.CombinationObject.Element?.Element.Element != null):
					referencedComboElements.Add(f3.CombinationObject.Element.Element.Element.Id);
					break;
				case StoragePickUpCombinationToInentoryByTemplateWithDropFunction f4 when (f4.CombinationObject.Element?.Element.Element != null):
					referencedComboElements.Add(f4.CombinationObject.Element.Element.Element.Id);
					break;
				case StoragePickUpCombinationWithDropFunction f5 when (f5.CombinationObject.Element?.Element.Element != null):
					referencedComboElements.Add(f5.CombinationObject.Element.Element.Element.Id);
					break;
			}
		}
		
		foreach (var expression in expressions) {
			if (expression.Function is GlobalStorageManagerIsStorableExistInCombinationFunction func && func.Combination.Element?.Element.Element != null) {
				referencedComboElements.Add(func.Combination.Element.Element.Element.Id);
			}
		}

		int deletedCount = 0;
		foreach (var combo in combosToDelete) {
			if (referencedComboElements.Contains(combo.Id)) continue;
			if (stringReferences.Any(s => s.Contains(combo.ParamId))) continue;
			
			Logger.Log(LogLevel.Info, $"Deleted unused combination '{combo.Name}' (ParamId: {combo.ParamId})");
			Vm.RemoveElement(combo);
			deletedCount++;
		}
		
		Logger.Log(LogLevel.Info, $"Completed: Deleted {deletedCount} unused combinations.");
	}
}
