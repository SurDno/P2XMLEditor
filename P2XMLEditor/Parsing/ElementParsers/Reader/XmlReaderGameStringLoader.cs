using System.Xml;
using P2XMLEditor.Helper;
using P2XMLEditor.Logging;
using P2XMLEditor.Parsing.RawData;
using static P2XMLEditor.Helper.XmlReaderExtensions;

namespace P2XMLEditor.Parsing.ElementParsers.Reader;

public class XmlReaderGameStringLoader : IParser<RawGameStringData> {
	[PerformanceLogHook]
	public void ProcessFile(string filePath, List<RawGameStringData> raws) {
		using var xr = InitializeFullFileReader(filePath);
		xr.SkipDeclarationAndRoot();

		while (xr.Read()) {
			if (xr.EndOfContainerReached()) break;
			var raw = new RawGameStringData {
				Id = xr.GetIdAndEnter(),
				ParentId = xr.GetULongValueAndAdvance()
			};
			raws.Add(raw);
		}
	}
}