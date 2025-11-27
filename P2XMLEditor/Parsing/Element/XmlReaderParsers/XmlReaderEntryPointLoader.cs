using P2XMLEditor.Helper;
using P2XMLEditor.Logging;
using P2XMLEditor.Parsing.RawData;
using static P2XMLEditor.Helper.XmlReaderExtensions;

namespace P2XMLEditor.Parsing.Element.XmlReaderParsers;

public class XmlReaderEntryPointLoader : IParser<RawEntryPointData> {
	[PerformanceLogHook]
	public void ProcessFile(string filePath, List<RawEntryPointData> raws) {
		using var xr = InitializeFullFileReader(filePath);
		xr.SkipDeclarationAndRoot();

		while (xr.Read()) {
			if (xr.EndOfContainerReached()) break;
			var raw = new RawEntryPointData {
				Id = xr.GetIdAndEnter(),
				Name = xr.GetStringValueAndAdvance(),
				ActionLineId = xr.Name == "ActionLine" ? xr.GetULongValueAndAdvance() : null,
				ParentId = xr.GetULongValueAndAdvance()
			};
			raws.Add(raw);
		}
	}
}