using P2XMLEditor.Enums.VirtualMachine;

namespace P2XMLEditor.Parsing.RawData;

public struct RawGameRootData {
	public ulong Id;
	public ulong[] SampleIds;
	public ulong[] LogicMapIds;
	public ulong[] GameModeIds;
	public Dictionary<string, string> BaseToEngineGuidsTable;
	public Dictionary<ulong, Dictionary<ChildContainerType, ulong[]>> HierarchyScenesStructure;
	public string[] HierarchyEngineGuidsTable;
	public bool? WorldObjectSaveOptimizeMode;
	public ulong[] FunctionalComponentIds;
	public ulong? EventGraphId;
	public ulong[]? ChildObjectIds;
	public ulong[]? EventIds;
	public (string, ulong)[] CustomParamIds;
	public (string, ulong)[] StandartParamIds;
	public string Name;

	public override int GetHashCode() => Id.GetHashCode();
}