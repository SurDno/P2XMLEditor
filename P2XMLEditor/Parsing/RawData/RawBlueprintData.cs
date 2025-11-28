using System.Collections.Generic;

namespace P2XMLEditor.Parsing.RawData;

public struct RawBlueprintData {
	public ulong Id;
	public bool? Static;
	public List<ulong> FunctionalComponentIds;
	public ulong? EventGraphId;
	public Dictionary<string, ulong> StandartParamIds;
	public Dictionary<string, ulong> CustomParamIds;
	public string? GameTimeContext;
	public string Name;
	public ulong? ParentId;
	public List<string>? InheritanceInfo;
	public List<ulong>? EventIds;
	public List<ulong>? ChildObjectIds;

	public override int GetHashCode() => Id.GetHashCode();
}