using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.GameData.VirtualMachineElements.Enums;

namespace P2XMLEditor.Suggestions.Refactoring;

[Refactoring("Refactor/Merge nested conditions")]
[SuppressMessage("ReSharper", "UnusedType.Global")]
public class MergeNestedConditions(VirtualMachine vm) : Suggestion(vm) {
	public override void Execute() {
		var conditions = Vm.GetElementsByType<Condition>();
		var subcondsToRemove = new List<Condition>();
        
		foreach (var cond in conditions) {
			var valuePredicates = cond.Predicates;
			if (cond.Predicates.Count <= 1) continue;
			var predicateConditions = valuePredicates.Select(p => p.Element).OfType<Condition>();
			if (predicateConditions.Count() == 0 || predicateConditions.Any(nc => nc!.Operation != cond.Operation))
				continue;
			var otherPredicates = valuePredicates.Where(p => p.Element is not Condition);
			cond.Predicates = otherPredicates.Concat(predicateConditions.SelectMany(nc => nc.Predicates)).ToList();
			foreach (var subcond in predicateConditions) {
				subcond!.Predicates = [];
				subcond.Operation = (ConditionOperation)int.MaxValue;
				subcond.Name = null!;
			}
			foreach (var pc in predicateConditions)
				subcondsToRemove.Add(pc);
		}
        
		foreach(var subcond in subcondsToRemove)
			Vm.RemoveElement(subcond);
	}
}