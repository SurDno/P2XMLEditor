namespace P2XMLEditor.Parsing.RawData;

public struct RawGameStringData {
	public ulong Id;
	public ulong ParentId;

	public override int GetHashCode() => Id.GetHashCode();
}