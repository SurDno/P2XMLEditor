using P2XMLEditor.Parsing.RawData;
using static P2XMLEditor.Parsing.Helpers.XmlReaderExtensions;

namespace P2XMLEditor.Parsing.Element.XmlReaderParsers;

public class XmlReaderGameStringLoader : IParser<RawGameStringData> {

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