namespace P2XMLEditor.Parsing.RawData;

public struct RawTalkingData {
	public ulong Id;
	public ulong[] StateIds;
	public ulong[] EventLinkIds;
	public ulong[] EntryPointIds;
	public bool? IgnoreBlock;
	public ulong OwnerId;
	public bool? Initial;
	public string Name;
	public ulong ParentId;

	public override int GetHashCode() => Id.GetHashCode();
}
