using P2XMLEditor.Abstract;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.GameData.VirtualMachineElements.Enums;

namespace P2XMLEditor.Refactoring;

public class RemoveEmptyEventGraphs(VirtualMachine vm) : RefactoringSuggestion(vm) {
	public override void Execute() {
		var items = _vm.GetElementsByType<Item>();
		
		foreach (var item in items) {
			var itemGraph = item.EventGraph;
			if (itemGraph == null) continue;
			vm.RemoveElement(itemGraph);
		}
	}
}