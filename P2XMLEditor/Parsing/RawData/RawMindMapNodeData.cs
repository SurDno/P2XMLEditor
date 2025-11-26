namespace P2XMLEditor.Parsing.RawData;

public struct RawMindMapNodeData {
	public ulong Id;
	public string LogicMapNodeType;
	public List<ulong>? ContentIds;
	public float GameScreenPosX;
	public float GameScreenPosY;
	public List<ulong>? InputLinkIds;
	public List<ulong>? OutputLinkIds;
	public string Name;
	public ulong ParentId;

	public override int GetHashCode() => Id.GetHashCode();
}