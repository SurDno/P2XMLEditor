namespace P2XMLEditor.Parsing.RawData;

public struct RawTalkingData {
	public ulong Id;
	public List<ulong> StateIds;
	public List<ulong> EventLinkIds;
	public List<ulong> EntryPointIds;
	public bool? IgnoreBlock;
	public ulong OwnerId;
	public bool? Initial;
	public string Name;
	public ulong ParentId;

	public override int GetHashCode() => Id.GetHashCode();
}