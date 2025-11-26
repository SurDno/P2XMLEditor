using System.Xml;
using P2XMLEditor.Helper;
using P2XMLEditor.Logging;
using P2XMLEditor.Parsing.RawData;
using static P2XMLEditor.Helper.XmlReaderExtensions;

namespace P2XMLEditor.Parsing.ElementParsers.Reader;

public class XmlReaderMindMapLinkLoader : IParser<RawMindMapLinkData> {
	[PerformanceLogHook]
	public void ProcessFile(string filePath, List<RawMindMapLinkData> raws) {
		using var xr = InitializeFullFileReader(filePath);
		xr.SkipDeclarationAndRoot();

		while (xr.Read()) {
			if (xr.EndOfContainerReached()) break;

			var raw = new RawMindMapLinkData {
				Id = xr.GetIdAndEnter(),
				SourceId = xr.GetULongValueAndAdvance(),
				DestinationId = xr.GetULongValueAndAdvance()
			};
			xr.SkipEmptyELement();
			raw.ParentId = xr.GetULongValueAndAdvance();

			raws.Add(raw);
		}
	}
}