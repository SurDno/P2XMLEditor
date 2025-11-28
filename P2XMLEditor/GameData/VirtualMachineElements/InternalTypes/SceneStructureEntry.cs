using System.Collections.Generic;
using P2XMLEditor.GameData.VirtualMachineElements.Enums;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes;

public struct SceneStructureEntry() {
	public Dictionary<ChildContainerType, List<ulong>> Containers { get; set; } = new();
}