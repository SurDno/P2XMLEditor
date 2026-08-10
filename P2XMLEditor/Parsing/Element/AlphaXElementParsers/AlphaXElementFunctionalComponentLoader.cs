using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;
using P2XMLEditor.Logging;
using P2XMLEditor.Parsing.Helpers;
using P2XMLEditor.Parsing.RawData;
using static P2XMLEditor.Parsing.Helpers.XElementExtensions;

namespace P2XMLEditor.Parsing.Element.AlphaXElementParsers;

public class AlphaXElementFunctionalComponentLoader : IParser<RawFunctionalComponentData> {
	
	[PerformanceLogHook]
	public void ProcessFile(string filePath, List<RawFunctionalComponentData> raws) {
		using var xr = XmlReaderExtensions.InitializeFullFileReader(filePath);

		xr.MoveToContent();
		xr.ReadStartElement();
		
		while (xr.NodeType == XmlNodeType.Element) {
			var element = (XElement)XNode.ReadFrom(xr);
			var id = ulong.Parse(element.Element("Guid")!.Value);

			var raw = new RawFunctionalComponentData {
				Id = id,
				EventIds = ParseDemoListAsUlong(element, "Events").ToArray(),
				Main = element.Element("Main")?.Let(ParseBool) ?? false,
				Name = element.Element("Name")!.Value,
				ParentId = ulong.Parse(element.Element("Parent")!.Value),
				LoadPriority = long.Parse(element.Element("LoadPriority")!.Value)
			};

			raws.Add(raw);
		}
	}
}
