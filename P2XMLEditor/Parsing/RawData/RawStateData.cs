using System.Collections.Generic;

namespace P2XMLEditor.Parsing.RawData;

public struct RawStateData {
	public ulong Id;
	public List<ulong> EntryPointIds;
	public bool? IgnoreBlock;
	public ulong OwnerId;
	public List<ulong>? InputLinkIds;
	public List<ulong>? OutputLinkIds;
	public bool? Initial;
	public string Name;
	public ulong ParentId;

	public override int GetHashCode() => Id.GetHashCode();
}