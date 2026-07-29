using P2XMLEditor.GameData.VirtualMachineElements.Interfaces;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes;

public class Message(string name)  {
	private string Name { get; } = name;
	public string ParamId => Name; 
}
