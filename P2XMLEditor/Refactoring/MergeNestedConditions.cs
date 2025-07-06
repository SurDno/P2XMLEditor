using System.Diagnostics.CodeAnalysis;
using P2XMLEditor.Abstract;
using P2XMLEditor.Attributes;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.GameData.VirtualMachineElements.Enums;

namespace P2XMLEditor.Refactoring;

[Refactoring("Refactor/Merge nested conditions")]
[SuppressMessage("ReSharper", "UnusedType.Global")]
public class MergeNestedConditions(VirtualMachine vm) : Suggestion(vm) {
	public override void Execute() {
		var conditions = Vm.GetElementsByType<Condition>();
		List<Condition> subcondsToRemove = [];
        
		foreach (var cond in conditions) {
			if (cond.Predicates.Count <= 1) continue;
			var predicateConditions = cond.Predicates.Select(p => p.Element).OfType<Condition>().ToList();
			if (predicateConditions.Count == 0 || predicateConditions.Any(nc => nc!.Operation != cond.Operation))
				continue;
			var otherPredicates = cond.Predicates.Where(p => p.Element is not Condition).ToList();
			cond.Predicates = otherPredicates.Concat(predicateConditions.SelectMany(nc => nc.Predicates)).ToList();
			foreach (var subcond in predicateConditions) {
				subcond!.Predicates = [];
				subcond.Operation = (ConditionOperation)int.MaxValue;
				subcond.Name = null!;
			}
			subcondsToRemove.AddRange(predicateConditions);
		}
        
		foreach(var subcond in subcondsToRemove)
			Vm.RemoveElement(subcond);
	}
}