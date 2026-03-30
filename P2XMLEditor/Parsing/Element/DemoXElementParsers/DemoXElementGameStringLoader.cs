using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using P2XMLEditor.Logging;
using P2XMLEditor.Parsing.Helpers;
using P2XMLEditor.Parsing.RawData;

namespace P2XMLEditor.Parsing.Element.DemoXElementParsers;

public class DemoXElementGameStringLoader : IParser<RawGameStringData> {
	
	[PerformanceLogHook]
	public void ProcessFile(string filePath, List<RawGameStringData> raws) {
		using var xr = XmlReaderExtensions.InitializeFullFileReader(filePath);

		xr.MoveToContent();
		xr.ReadStartElement();
		
		while (xr.NodeType == XmlNodeType.Element) {
			var element = (XElement)XNode.ReadFrom(xr);
			var id = ulong.Parse(element.Attribute("id")!.Value);

			Dictionary<string, string>? languageTexts = null;
			var dictElement = element.Element("language_strings.Dict");
			if (dictElement != null) {
				languageTexts = dictElement.Elements()
					.ToDictionary(e => e.Name.LocalName, e => e.Value);
			}

			var raw = new RawGameStringData {
				Id = id,
				ParentId = ulong.Parse(element.Element("Parent")!.Value),
				LanguageTexts = languageTexts
			};

			raws.Add(raw);
		}
	}
}
