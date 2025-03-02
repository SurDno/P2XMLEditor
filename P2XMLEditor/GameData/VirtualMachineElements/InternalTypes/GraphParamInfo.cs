using P2XMLEditor.Core;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes;

public record struct GraphParamInfo(string Name, string Type) {
	public string Name { get; } = Name;
	public string Type { get; } = Type;
	public InputParameter? InputParameter { get; set; } = null;


	public void ResolveReferences(VirtualMachine vm) {
		if (InputParameter.TryParse(Name, vm, out var inputParam))
			InputParameter = inputParam;
	}
}