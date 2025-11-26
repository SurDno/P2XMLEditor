namespace P2XMLEditor.Parsing.RawData;

public struct RawGameObjectData {
	public ulong Id;
	public string? WorldPositionGuid;
	public string? EngineTemplateId;
	public string? EngineBaseTemplateId;
	public bool? Instantiated;
	public bool? Static;
	public List<string>? InheritanceInfo;
	public List<ulong>? FunctionalComponentIds;
	public ulong? EventGraphId;
	public List<ulong>? ChildObjectIds;
	public List<ulong>? EventIds;
	public Dictionary<string, ulong>? CustomParamIds;
	public Dictionary<string, ulong>? StandartParamIds;
	public string? GameTimeContext;
	public string Name;
	public ulong ParentId;

	public override int GetHashCode() => Id.GetHashCode();
}