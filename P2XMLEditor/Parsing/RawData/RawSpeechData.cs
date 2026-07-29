namespace P2XMLEditor.Parsing.RawData;

public struct RawSpeechData {
	public ulong Id;
	public ulong[] ReplyIds;
	public ulong TextId;
	public ulong AuthorGuidId;
	public bool? OnlyOnce;
	public bool? IsTrade;
	public ulong[] EntryPointIds;
	public bool? IgnoreBlock;
	public ulong OwnerId;
	public ulong[]? InputLinkIds;
	public ulong[]? OutputLinkIds;
	public bool? Initial;
	public string Name;
	public ulong ParentId;

	public override int GetHashCode() => Id.GetHashCode();
}
