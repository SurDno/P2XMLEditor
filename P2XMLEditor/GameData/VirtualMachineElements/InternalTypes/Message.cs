using P2XMLEditor.GameData.VirtualMachineElements.Interfaces;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes;

public class Message(string name) : ICommonVariableParameter {
	private string Name { get; } = name;
	public string Id => Name; 
}
