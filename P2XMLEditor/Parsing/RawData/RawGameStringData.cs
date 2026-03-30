using System.Collections.Generic;

namespace P2XMLEditor.Parsing.RawData;

public struct RawGameStringData {
	public ulong Id;
	public ulong ParentId;
	public Dictionary<string, string>? LanguageTexts; // Demo-only in RawData

	public override int GetHashCode() => Id.GetHashCode();
}