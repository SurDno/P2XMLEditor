using System.Diagnostics.CodeAnalysis;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.Suggestions.Abstract;
using P2XMLEditor.Suggestions.Attributes;

namespace P2XMLEditor.Suggestions.Refactoring;

[Refactoring("Refactor/Event Graphs/Remove empty event graphs")]
[SuppressMessage("ReSharper", "UnusedType.Global")]
public class RemoveEmptyEventGraphs(VirtualMachine vm) : Suggestion(vm) {
	public override void Execute() {
		var items = Vm.GetElementsByType<Item>();
		
		foreach (var item in items) {
			var itemGraph = item.EventGraph;
			if (itemGraph == null) continue;
			Vm.RemoveElement(itemGraph);
		}
	}
}