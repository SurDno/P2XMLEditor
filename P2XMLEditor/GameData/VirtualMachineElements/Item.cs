using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.Helper;

namespace P2XMLEditor.GameData.VirtualMachineElements;

public class Item(ulong id) : GameObject(id) {
	public void OnDestroy(VirtualMachine vm) {
		CombinationHelper.RemoveFromPotentialCombinations(vm, this);
		base.OnDestroy(vm);
	}
}