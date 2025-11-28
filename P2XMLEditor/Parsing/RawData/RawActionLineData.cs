using System.Collections.Generic;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.GameData.VirtualMachineElements.Enums;

namespace P2XMLEditor.Parsing.RawData;

public struct RawActionLineData {
	public ulong Id;
	public List<ulong>? ActionIds;
	public ActionLineType ActionLineType;
	public ActionLine.ActionLoopInfo? LoopInfo;
	public string Name;
	public ulong LocalContextId;
	public int OrderIndex;

	public override int GetHashCode() => Id.GetHashCode();
}