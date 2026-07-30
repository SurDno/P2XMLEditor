namespace P2XMLEditor.Parsing.RawData;

public struct RawQuestData {
	public ulong Id;
	public ulong? StartEventId;
	public bool Static;
	public string[]? InheritanceInfo;
	public ulong[] FunctionalComponentIds;
	public ulong EventGraphId;
	public ulong[]? ChildObjectIds;
	public ulong[]? EventIds;
	public (string, ulong)[] CustomParamIds;
	public (string, ulong)[] StandartParamIds;
	public string? GameTimeContext;
	public string Name;
	public ulong ParentId;

	public override int GetHashCode() => Id.GetHashCode();
}
