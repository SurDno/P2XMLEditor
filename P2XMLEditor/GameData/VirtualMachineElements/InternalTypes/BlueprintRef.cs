using P2XMLEditor.Core;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes;

public class BlueprintRef {
	public VmEither<Item, Other, Character> Element { get; set; }
	public bool SerializeAsGuid { get; set; }
}
