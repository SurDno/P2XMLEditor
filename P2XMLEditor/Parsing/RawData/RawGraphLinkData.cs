using System.Collections.Generic;

namespace P2XMLEditor.Parsing.RawData;

public struct RawGraphLinkData {
	public ulong Id;
	public ulong? EventId;
	public string EventObject;
	public int SourceExitPointIndex;
	public int DestEntryPointIndex;
	public List<string>? SourceParams;
	public ulong? SourceId;
	public ulong? DestinationId;
	public bool? Enabled;
	public string Name;
	public ulong ParentId;

	public override int GetHashCode() => Id.GetHashCode();
}