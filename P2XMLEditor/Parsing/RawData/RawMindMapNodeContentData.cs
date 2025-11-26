namespace P2XMLEditor.Parsing.RawData;

public struct RawMindMapNodeContentData {
	public ulong Id;
	public string ContentType;
	public int Number;
	public ulong ContentDescriptionTextId;
	public ulong? ContentPictureId;
	public ulong ContentConditionId;
	public string Name;
	public ulong ParentId;

	public override int GetHashCode() => Id.GetHashCode();
}