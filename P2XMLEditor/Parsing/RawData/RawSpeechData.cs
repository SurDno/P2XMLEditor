namespace P2XMLEditor.Parsing.RawData;

public struct RawSpeechData {
	public ulong Id;
	public List<ulong> ReplyIds;
	public ulong TextId;
	public ulong AuthorGuidId;
	public bool? OnlyOnce;
	public bool? IsTrade;
	public List<ulong> EntryPointIds;
	public bool? IgnoreBlock;
	public ulong OwnerId;
	public List<ulong>? InputLinkIds;
	public List<ulong>? OutputLinkIds;
	public bool? Initial;
	public string Name;
	public ulong ParentId;

	public override int GetHashCode() => Id.GetHashCode();
}