using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using P2XMLEditor.Logging;
using P2XMLEditor.Parsing.Helpers;
using P2XMLEditor.Parsing.RawData;
using static P2XMLEditor.Parsing.Helpers.XElementExtensions;

namespace P2XMLEditor.Parsing.Element.DemoXElementParsers;

public class DemoXElementBlueprintLoader : IParser<RawBlueprintData> {
	[PerformanceLogHook]
	public void ProcessFile(string filePath, List<RawBlueprintData> raws) {
		using var xr = XmlReaderExtensions.InitializeFullFileReader(filePath);

		xr.MoveToContent();
		xr.ReadStartElement();
		
		while (xr.NodeType == XmlNodeType.Element) {
			var element = (XElement)XNode.ReadFrom(xr);
			var id = ulong.Parse(element.Attribute("id")!.Value);

			var raw = new RawBlueprintData {
				Id = id,
				Static = element.Element("Static")?.Let(ParseBool),
				FunctionalComponentIds = ParseDemoListAsUlong(element, "FunctionalComponents").ToArray(),
				EventGraphId = element.Element("EventGraph") != null ?
					ulong.Parse(element.Element("EventGraph")!.Value) : null,
				StandartParamIds = ParseDemoDictAsUlong(element, "StandartParams"),
				CustomParamIds = ParseDemoDictAsUlong(element, "CustomParams"),
				GameTimeContext = element.Element("GameTimeContext")?.Value,
				Name = element.Element("Name")!.Value,
				ParentId = element.Element("Parent") != null ?
					ulong.Parse(element.Element("Parent")!.Value) : null,
				InheritanceInfo = ParseDemoList(element, "InheritanceInfo").ToArray(),
				EventIds = ParseDemoListAsUlong(element, "Events").ToArray(),
				ChildObjectIds = ParseDemoListAsUlong(element, "ChildObjects").ToArray()
			};

			raws.Add(raw);
		}
	}
}
