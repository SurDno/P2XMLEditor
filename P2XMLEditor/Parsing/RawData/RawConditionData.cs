using System.Collections.Generic;
using P2XMLEditor.GameData.VirtualMachineElements.Enums;

namespace P2XMLEditor.Parsing.RawData;

public struct RawConditionData {
	public ulong Id;
	public List<ulong> PredicateIds;
	public ConditionOperation Operation;
	public string Name;
	public int OrderIndex;

	public override int GetHashCode() => Id.GetHashCode();
}