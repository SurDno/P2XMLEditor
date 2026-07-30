using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using P2XMLEditor.Logging;
using P2XMLEditor.Parsing.Helpers;
using P2XMLEditor.Parsing.RawData;
using static P2XMLEditor.Parsing.Helpers.XElementExtensions;

namespace P2XMLEditor.Parsing.Element.DemoXElementParsers;

public class DemoXElementSpeechLoader : IParser<RawSpeechData> {
	[PerformanceLogHook]
	public void ProcessFile(string filePath, List<RawSpeechData> raws) {
		using var xr = XmlReaderExtensions.InitializeFullFileReader(filePath);

		xr.MoveToContent();
		xr.ReadStartElement();
		
		while (xr.NodeType == XmlNodeType.Element) {
			var element = (XElement)XNode.ReadFrom(xr);
			var id = ulong.Parse(element.Attribute("id")!.Value);

			var raw = new RawSpeechData {
				Id = id,
				ReplyIds = ParseDemoListAsUlong(element, "Replyes").ToArray(),
				TextId = ulong.Parse(element.Element("Text")!.Value),
				AuthorGuidId = ulong.Parse(element.Element("AuthorGuid")!.Value),
				OnlyOnce = element.Element("OnlyOnce") != null
					? bool.Parse(element.Element("OnlyOnce")!.Value)
					: false,
				IsTrade = element.Element("IsTrade") != null
					? bool.Parse(element.Element("IsTrade")!.Value)
					: false,
				EntryPointIds = ParseDemoListAsUlong(element, "EntryPoints").ToArray(),
				IgnoreBlock = element.Element("IgnoreBlock") != null
					? bool.Parse(element.Element("IgnoreBlock")!.Value)
					: false,
				OwnerId = ulong.Parse(element.Element("Owner")!.Value),
				InputLinkIds = element.Element("InputLinks.List") != null
					? ParseDemoListAsUlong(element, "InputLinks").ToArray()
					: null,
				OutputLinkIds = element.Element("OutputLinks.List") != null
					? ParseDemoListAsUlong(element, "OutputLinks").ToArray()
					: null,
				Initial = element.Element("Initial") != null
					? bool.Parse(element.Element("Initial")!.Value)
					: false,
				Name = element.Element("Name")!.Value,
				ParentId = ulong.Parse(element.Element("Parent")!.Value)
			};

			raws.Add(raw);
		}
	}
}
