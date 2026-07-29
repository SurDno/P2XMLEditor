using P2XMLEditor.GameData.VirtualMachineElements.Interfaces;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes;

public abstract class ConstantParameter  {
	public abstract string ParamId { get; }
	public abstract object? GetValue();
}
