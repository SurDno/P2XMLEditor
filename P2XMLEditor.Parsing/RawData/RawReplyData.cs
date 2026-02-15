namespace P2XMLEditor.Parsing.RawData;

public struct RawReplyData {
	public ulong Id;
	public string Name;
	public ulong TextId;
	public bool? OnlyOnce;
	public bool? OnlyOneReply;
	public bool? Default;
	public ulong? EnableConditionId;
	public ulong? ActionLineId;
	public int OrderIndex;
	public ulong ParentId;

	public override int GetHashCode() => Id.GetHashCode();
}