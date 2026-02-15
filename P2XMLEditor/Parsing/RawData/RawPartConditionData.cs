namespace P2XMLEditor.Parsing.RawData;

public struct RawPartConditionData {
	public ulong Id;
	public string? Name;
	public string ConditionType;
	public ulong? FirstExpressionId;
	public ulong? SecondExpressionId;
	public int OrderIndex;

	public override int GetHashCode() => Id.GetHashCode();
}