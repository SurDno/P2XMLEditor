using P2XMLEditor.GameData.VirtualMachineElements.Abstract;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes;

public class EntityRef {
	public ParameterHolder? Element { get; set; }
	public bool SerializeAsGuid { get; set; }
}
