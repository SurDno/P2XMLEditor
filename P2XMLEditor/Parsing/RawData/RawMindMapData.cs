using P2XMLEditor.GameData.VirtualMachineElements.Enums;

namespace P2XMLEditor.Parsing.RawData;

public struct RawMindMapData {
	public ulong Id;
	public List<ulong> NodeIds;
	public List<ulong>? LinkIds;
	public LogicMapType LogicMapType;
	public ulong TitleId;
	public string Name;
	public ulong ParentId;

	public override int GetHashCode() => Id.GetHashCode();
}