using P2XMLEditor.Core;
using P2XMLEditor.Forms.MainForm.Combinations;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.Helper;

namespace P2XMLEditor.GameData.VirtualMachineElements;

public class Other(string id) : GameObject(id) { 
	public override void OnDestroy(VirtualMachine vm) {
		CombinationHelper.RemoveFromPotentialCombinations(vm, this);
		base.OnDestroy(vm);
	}
}