using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes;

namespace P2XMLEditor.Parsing.RawData;

public struct RawGameRootData {
	public ulong Id;
	public List<ulong> SampleIds;
	public List<ulong> LogicMapIds;
	public List<ulong> GameModeIds;
	public Dictionary<string, string> BaseToEngineGuidsTable;
	public Dictionary<ulong, SceneStructureEntry> HierarchyScenesStructure;
	public List<string> HierarchyEngineGuidsTable;
	public bool? WorldObjectSaveOptimizeMode;
	public List<ulong> FunctionalComponentIds;
	public ulong? EventGraphId;
	public List<ulong>? ChildObjectIds;
	public List<ulong>? EventIds;
	public Dictionary<string, ulong> CustomParamIds;
	public Dictionary<string, ulong> StandartParamIds;
	public string Name;

	public override int GetHashCode() => Id.GetHashCode();
}