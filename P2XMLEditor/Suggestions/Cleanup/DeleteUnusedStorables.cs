using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions.Actions.GlobalMarketManager;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions.Actions.GlobalStorageManager;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions.Actions.Storage;
using P2XMLEditor.Helper;
using P2XMLEditor.Logging;
using Action = P2XMLEditor.GameData.VirtualMachineElements.Action;

namespace P2XMLEditor.Suggestions.Cleanup;

// TODO: completely refactor once we start storing Action function references normally. 
[Cleanup("References/Game Objects/Delete unused storables"), SuppressMessage("ReSharper", "UnusedType.Global")]
public class DeleteUnusedStorables(VirtualMachine vm) : Suggestion(vm) {
	public override void Execute() {
		var all = Vm.GetElementsByType<Item>().Cast<GameObject>().Concat(Vm.GetElementsByType<Other>()).ToList();
		var combos = all.Where(i => i.StandartParams.ContainsKey(CombinationHelper.CombinationKey)).ToList();
		var items = all.Where(i => i.StandartParams.ContainsKey(CombinationHelper.StorableKey)).Except(combos).ToList();

		var parameters = Vm.GetElementsByType<Parameter>().ToList();
		var actions = Vm.GetElementsByType<Action>().ToList();
		var parameterHolders = vm.GetElementsByType<ParameterHolder>().ToList();
		
		var storeablesToDelete = items.Where(item => CombinationHelper.GetCombinationsWithItem(Vm, item).Count == 0).ToList();
		var referencedStorableIds = new System.Collections.Generic.HashSet<string>(System.StringComparer.OrdinalIgnoreCase);
		foreach (var parameter in parameters) {
			var values = parameter.Value is CommonListValue list ? list.TypedValue : [parameter.Value];
			foreach (var v in values) {
				if (v is BasicValue<Guid> g) {
					referencedStorableIds.Add(g.TypedValue.ToString("N"));
				}
			}
		}

		var referencedStorableElements = new System.Collections.Generic.HashSet<ParameterHolder>();
		foreach (var action in actions) {
			switch (action.Function) {
				case StoragePickUpByTemplateFunction { Template.Element.Element: not null } f1:
					referencedStorableElements.Add(f1.Template.Element.Element);
					break;
				case StoragePickUpByTemplate_v1Function { Template.Element.Element: not null } f2:
					referencedStorableElements.Add(f2.Template.Element.Element);
					break;
				case StoragePickUpToInentoryByTemplateFunction { Template.Element.Element: not null } f3:
					referencedStorableElements.Add(f3.Template.Element.Element);
					break;
				case StorageRemoveThingByTemplateFunction { ItemTemplate.Element.Element: not null } f4:
					referencedStorableElements.Add(f4.ItemTemplate.Element.Element);
					break;
				case GlobalMarketManagerSetBaseItemTradePriceFactorsFunction f5 :
					var itemNames = f5.ItemEntity.Write().TrimStart('%').Split(';');
					foreach (var name in itemNames) {
						var target = parameterHolders.First(p => p.Name == name);
						referencedStorableElements.Add(target);
					}

					break;
			}
		}

		int deletedCount = 0;
		foreach (var storable in storeablesToDelete) {
			if (storable.EngineTemplateId != null && referencedStorableIds.Contains(storable.EngineTemplateId)) continue;
			if (referencedStorableElements.Contains(storable)) continue;
			
			Logger.Log(LogLevel.Info, $"Deleted unused storable '{storable.Name}' (EngineTemplateId: {storable.EngineTemplateId})");
			Vm.RemoveElement(storable);
			deletedCount++;
		}
		
		Logger.Log(LogLevel.Info, $"Completed: Deleted {deletedCount} unused storables.");
	}
}
