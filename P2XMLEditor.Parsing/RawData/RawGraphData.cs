namespace P2XMLEditor.Parsing.RawData;

public struct RawGraphData {
	public ulong Id;
	public ulong[]? StateIds;
	public ulong[]? EventLinkIds;
	public ulong? SubstituteGraphId;
	public string GraphType;
	public (string, string)[]? InputParamsInfo;
	public ulong[]? EntryPointIds;
	public bool? IgnoreBlock;
	public ulong OwnerId;
	public ulong[]? InputLinkIds;
	public ulong[]? OutputLinkIds;
	public bool? Initial;
	public string Name;
	public ulong ParentId;

	public override int GetHashCode() => Id.GetHashCode();
}