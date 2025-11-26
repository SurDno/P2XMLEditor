namespace P2XMLEditor.Parsing.RawData;

public struct RawMindMapLinkData {
	public ulong Id;
	public ulong SourceId;
	public ulong DestinationId;
	public ulong ParentId;

	public override int GetHashCode() => Id.GetHashCode();
}