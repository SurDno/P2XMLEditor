using P2XMLEditor.GameData.VirtualMachineElements.Enums;

namespace P2XMLEditor.Parsing.RawData;

public struct RawMindMapNodeData {
	public ulong Id;
	public LogicMapNodeType LogicMapNodeType;
	public ulong[]? ContentIds;
	public float GameScreenPosX;
	public float GameScreenPosY;
	public ulong[]? InputLinkIds;
	public ulong[]? OutputLinkIds;
	public string Name;
	public ulong ParentId;

	public override int GetHashCode() => Id.GetHashCode();
}