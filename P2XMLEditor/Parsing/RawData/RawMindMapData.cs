using P2XMLEditor.GameData.VirtualMachineElements.Enums;

namespace P2XMLEditor.Parsing.RawData;

public struct RawMindMapData {
	public ulong Id;
	public ulong[] NodeIds;
	public ulong[]? LinkIds;
	public LogicMapType LogicMapType;
	public ulong TitleId;
	public string Name;
	public ulong ParentId;
	public ulong[]? TextObjectIds; // Demo-only
	public ulong? ParentFolder; // Demo-only

	public override int GetHashCode() => Id.GetHashCode();
}