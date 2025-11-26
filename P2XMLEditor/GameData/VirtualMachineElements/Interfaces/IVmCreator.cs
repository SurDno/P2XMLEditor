using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;

namespace P2XMLEditor.GameData.VirtualMachineElements.Interfaces;

public interface IVmCreator<out T> where T : VmElement {
	public static abstract T New(VirtualMachine vm, ulong id, VmElement parent);
}