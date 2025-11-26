namespace P2XMLEditor.Parsing.RawData;

public struct RawEntryPointData {
	public ulong Id;
	public string Name;
	public ulong? ActionLineId;
	public ulong ParentId;

	public override int GetHashCode() => Id.GetHashCode();
}