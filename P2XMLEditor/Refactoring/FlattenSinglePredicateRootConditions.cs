using System.Diagnostics.CodeAnalysis;
using P2XMLEditor.Abstract;
using P2XMLEditor.Attributes;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.GameData.VirtualMachineElements.Enums;

namespace P2XMLEditor.Refactoring;

[Refactoring("Refactor/Flatten single predicate root conditions")]
[SuppressMessage("ReSharper", "UnusedType.Global")]
public class FlattenSinglePredicateRootConditions(VirtualMachine vm) : Suggestion(vm) {
	public override void Execute() {
		var conditions = Vm.GetElementsByType<Condition>();

		List<Condition> subcondsToRemove = [];
		
		foreach (var cond in conditions) {
			if (cond.Operation is not ConditionOperation.Root || cond.Predicates.Count > 1) continue;
			var predicate = cond.Predicates[0].Element;
			if (predicate is not Condition subcond) continue;
			cond.Predicates = subcond.Predicates;
			cond.Operation = subcond.Operation;
			cond.Name = string.IsNullOrEmpty(subcond.Name) ? cond.Name : subcond.Name;
			subcond.Predicates = [];
			subcond.Operation = (ConditionOperation)int.MaxValue;
			subcond.Name = null!;
			subcondsToRemove.Add(subcond);
		}
		
		foreach(var subcond in subcondsToRemove)
			Vm.RemoveElement(subcond);
	}
}