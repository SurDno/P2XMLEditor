namespace P2XMLEditor.Parsing.RawData;

public struct RawFunctionalComponentData {
	public ulong Id;
	public List<ulong>? EventIds;
	public bool? Main;
	public long LoadPriority;
	public string Name;
	public ulong ParentId;

	public override int GetHashCode() => Id.GetHashCode();
}