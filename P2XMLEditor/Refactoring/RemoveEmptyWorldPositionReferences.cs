using System.Diagnostics.CodeAnalysis;
using P2XMLEditor.Abstract;
using P2XMLEditor.Attributes;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;

namespace P2XMLEditor.Refactoring;

[Refactoring("Refactor/Parameter Holders/Remove empty world position references")]
[SuppressMessage("ReSharper", "UnusedType.Global")]
public class RemoveEmptyWorldPositionReferences(VirtualMachine vm) : Suggestion(vm) {
	public override void Execute() {
		foreach (var item in Vm.GetElementsByType<GameObject>()) {
			if (item.WorldPositionGuid == string.Empty)
				item.WorldPositionGuid = null;
		}
	}
}