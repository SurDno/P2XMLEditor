using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes;

namespace P2XMLEditor.Parsing.RawData;

public struct RawGraphData {
	public ulong Id;
	public List<ulong>? StateIds;
	public List<ulong>? EventLinkIds;
	public ulong? SubstituteGraphId;
	public string GraphType;
	public List<GraphParamInfo>? InputParamsInfo;
	public List<ulong>? EntryPointIds;
	public bool? IgnoreBlock;
	public ulong OwnerId;
	public List<ulong>? InputLinkIds;
	public List<ulong>? OutputLinkIds;
	public bool? Initial;
	public string Name;
	public ulong ParentId;

	public override int GetHashCode() => Id.GetHashCode();
}