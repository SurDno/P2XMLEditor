using System.Diagnostics.CodeAnalysis;
using P2XMLEditor.Abstract;
using P2XMLEditor.Attributes;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;

namespace P2XMLEditor.Refactoring;

[Refactoring("Refactor/Parameter Holders/Remove empty GUID references")]
[SuppressMessage("ReSharper", "UnusedType.Global")]
public class RemoveEmptyGUIDReferences(VirtualMachine vm) : Suggestion(vm) {
	public override void Execute() {
		foreach (var item in _vm.GetElementsByType<GameObject>()) {
			if (item.EngineTemplateId == "00000000000000000000000000000000") 
				item.EngineTemplateId = null;
			if (item.EngineBaseTemplateId == "00000000000000000000000000000000") 
				item.EngineBaseTemplateId = null;
		}
	}
}