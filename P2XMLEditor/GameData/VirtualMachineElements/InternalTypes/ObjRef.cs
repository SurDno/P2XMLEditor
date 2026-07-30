using P2XMLEditor.GameData.VirtualMachineElements.Abstract;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes;

public class ObjRef {
	public ParameterHolder? StaticObject { get; set; }
	public HierarchyGuid? Hierarchy { get; set; }
	public string? EngineGuid { get; set; }
}