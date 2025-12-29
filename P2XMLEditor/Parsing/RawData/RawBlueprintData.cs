namespace P2XMLEditor.Parsing.RawData;

public struct RawBlueprintData {
	public ulong Id;
	public bool? Static;
	public ulong[] FunctionalComponentIds;
	public ulong? EventGraphId;
	public (string, ulong)[] StandartParamIds;
	public (string, ulong)[] CustomParamIds;
	public string? GameTimeContext;
	public string Name;
	public ulong? ParentId;
	public string[]? InheritanceInfo;
	public ulong[]? EventIds;
	public ulong[]? ChildObjectIds;

	public override int GetHashCode() => Id.GetHashCode();
}