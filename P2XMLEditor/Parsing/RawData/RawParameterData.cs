namespace P2XMLEditor.Parsing.RawData;

public struct RawParameterData {
	public ulong Id;
	public string Name;
	public ulong? OwnerComponentId;
	public string Type;
	public string Value;
	public bool? Implicit;
	public ulong ParentId;
	public bool? Custom;

	public override int GetHashCode() => Id.GetHashCode();
}