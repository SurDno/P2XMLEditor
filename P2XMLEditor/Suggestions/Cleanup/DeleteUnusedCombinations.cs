using System.Diagnostics.CodeAnalysis;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions;
using P2XMLEditor.Helper;
using P2XMLEditor.Logging;
using P2XMLEditor.Suggestions.Abstract;
using P2XMLEditor.Suggestions.Attributes;
using Action = P2XMLEditor.GameData.VirtualMachineElements.Action;

namespace P2XMLEditor.Suggestions.Cleanup;

// TODO: completely refactor once we start storing Action function references and Parameter types normally. 
[Cleanup("References/Delete unused combinations")]
[SuppressMessage("ReSharper", "UnusedType.Global")]
public class DeleteUnusedCombinations(VirtualMachine vm) : Suggestion(vm) {
	public override void Execute() {
		var combos = Vm.GetElementsByType<Item>().Cast<GameObject>().Concat(Vm.GetElementsByType<Other>()).ToList()
			.Where(i => i.StandartParams.ContainsKey(CombinationHelper.CombinationKey)).ToList();

		var expressions = Vm.GetElementsByType<Expression>().ToList();
		var actions = Vm.GetElementsByType<Action>().ToList();
		
		foreach (var combo in combos.Where(item => CombinationHelper.GetCombinationsWithItem(Vm, item).Count == 0)) {
			
			
			var referencedInActions = false;
			foreach (var action in actions.Where(action => 
				         action.TargetFuncName.Contains("PickUpCombinationToInentoryByTemplate") || 
				         action.TargetFuncName.Contains("AddItemsToStoragesLinear") ||
				         action.TargetFuncName.Contains("PickUpCombination"))) {
				if (action.SourceParams == null) continue;
				if (action.SourceParams.Any(p => p.Contains(combo.ParamId.ToString())))
					referencedInActions = true;
			}
			if (referencedInActions) continue;
			var referencedInExpressions = false;
			foreach (var expression in expressions) {
				if (expression.Function is not IsStorableExistInCombinationFunction func) continue;
				var sourceParams = func.GetParamStrings();
				if (sourceParams.Any(e => e.Contains(combo.ParamId.ToString())))
					referencedInExpressions = true;
			}
			if (referencedInExpressions) continue;
			
			
			Logger.Log(LogLevel.Info, $"Deleting unused combination {combo.ParamId} {combo.Name}.");
			Vm.RemoveElement(combo);
		}
	}
}
