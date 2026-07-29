using System.Diagnostics.CodeAnalysis;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.GameData.VirtualMachineElements.Enums;

namespace P2XMLEditor.Suggestions.Refactoring;

[Refactoring("Refactor/Remove constant reply conditions"), SuppressMessage("ReSharper", "UnusedType.Global")]
public class RemoveConstantReplyConditions(VirtualMachine vm) : Suggestion(vm) {
	public override void Execute() {
		var replies = Vm.GetElementsByType<Reply>();
		
		foreach (var reply in replies) {
			var replyEnableCondition = reply.EnableCondition;
			if (replyEnableCondition == null) continue;
			if (replyEnableCondition.Operation != ConditionOperation.Root) continue;
			
			var predicate = reply.EnableCondition!.Predicates[0].Element;
			if (predicate is not PartCondition pd) continue;
			if (pd.ConditionType is not ConditionType.ConstTrue) continue;
			
			Vm.RemoveElement(replyEnableCondition);
			reply.EnableCondition = null;
		}
		
	}
}
