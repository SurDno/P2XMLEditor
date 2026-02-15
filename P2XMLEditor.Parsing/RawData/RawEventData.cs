using P2XMLEditor.Enums.VirtualMachine;

namespace P2XMLEditor.Parsing.RawData;

public struct RawEventData {
	public ulong Id;
	public ulong? EventParameterId;
	public TimeSpan EventTime;
	public bool? Manual;
	public EventRaisingType EventRaisingType;
	public bool? ChangeTo;
	public bool? Repeated;
	public (string, string)[]? MessagesInfo;
	public string Name;
	public ulong ParentId;
	public ulong? ConditionId;

	public override int GetHashCode() => Id.GetHashCode();
}