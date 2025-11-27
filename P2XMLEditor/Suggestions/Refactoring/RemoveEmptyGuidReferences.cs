using System.Diagnostics.CodeAnalysis;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;

namespace P2XMLEditor.Suggestions.Refactoring;

[Refactoring("Refactor/Parameter Holders/Remove empty GUID references")]
[SuppressMessage("ReSharper", "UnusedType.Global")]
public class RemoveEmptyGuidReferences(VirtualMachine vm) : Suggestion(vm) {
	public override void Execute() {
		foreach (var item in Vm.GetElementsByType<GameObject>()) {
			if (item.EngineTemplateId == "00000000000000000000000000000000") 
				item.EngineTemplateId = null;
			if (item.EngineBaseTemplateId == "00000000000000000000000000000000") 
				item.EngineBaseTemplateId = null;
		}
	}
}