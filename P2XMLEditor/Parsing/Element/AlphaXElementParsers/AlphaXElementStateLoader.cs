using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using P2XMLEditor.Logging;
using P2XMLEditor.Parsing.Helpers;
using P2XMLEditor.Parsing.RawData;
using static P2XMLEditor.Parsing.Helpers.XElementExtensions;

namespace P2XMLEditor.Parsing.Element.AlphaXElementParsers;

public class AlphaXElementStateLoader : IParser<RawStateData> {
	[PerformanceLogHook]
	public void ProcessFile(string filePath, List<RawStateData> raws) {
		using var xr = XmlReaderExtensions.InitializeFullFileReader(filePath);

		xr.MoveToContent();
		xr.ReadStartElement();
		
		while (xr.NodeType == XmlNodeType.Element) {
			var element = (XElement)XNode.ReadFrom(xr);
			var id = ulong.Parse(element.Element("Guid")!.Value);

			var raw = new RawStateData {
				Id = id,
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
