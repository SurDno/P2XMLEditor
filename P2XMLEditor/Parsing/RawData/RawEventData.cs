using System;
using System.Collections.Generic;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.GameData.VirtualMachineElements.Enums;

namespace P2XMLEditor.Parsing.RawData;

public struct RawEventData {
	public ulong Id;
	public ulong? EventParameterId;
	public TimeSpan EventTime;
	public bool? Manual;
	public EventRaisingType EventRaisingType;
	public bool? ChangeTo;
	public bool? Repeated;
	public List<MessageInfo>? MessagesInfo;
	public string Name;
	public ulong ParentId;
	public ulong? ConditionId;

	public override int GetHashCode() => Id.GetHashCode();
}