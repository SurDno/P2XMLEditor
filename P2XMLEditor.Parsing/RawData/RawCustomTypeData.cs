namespace P2XMLEditor.Parsing.RawData;

public struct RawCustomTypeData {
	public ulong Id;
	public string Name;
	public ulong ParentId;

	public override int GetHashCode() => Id.GetHashCode();
}