using P2XMLEditor.GameData.VirtualMachineElements.Enums;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes;

public class SceneStructureEntry {
	public Dictionary<ChildContainerType, List<string>> Containers { get; set; } = new();
}