using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using P2XMLEditor.Logging;
using P2XMLEditor.Parsing.Helpers;
using P2XMLEditor.Parsing.RawData;
using static P2XMLEditor.Parsing.Helpers.XElementExtensions;

namespace P2XMLEditor.Parsing.Element.DemoXElementParsers;

public class DemoXElementQuestLoader : IParser<RawQuestData> {
	[PerformanceLogHook]
	public void ProcessFile(string filePath, List<RawQuestData> raws) {
		using var xr = XmlReaderExtensions.InitializeFullFileReader(filePath);

		xr.MoveToContent();
		xr.ReadStartElement();
		
		while (xr.NodeType == XmlNodeType.Element) {
			var element = (XElement)XNode.ReadFrom(xr);
			var id = ulong.Parse(element.Attribute("id")!.Value);

			var raw = new RawQuestData {
				Id = id,
				Static = element.Element("Static") != null
					? bool.Parse(element.Element("Static")!.Value)
					: false,
				FunctionalComponentIds = ParseDemoListAsUlong(element, "FunctionalComponents").ToArray(),
				EventGraphId = ulong.Parse(element.Element("EventGraph")!.Value),
				StandartParamIds = ParseDemoDictAsUlong(element, "StandartParams"),
				CustomParamIds = ParseDemoDictAsUlong(element, "CustomParams"),
				GameTimeContext = element.Element("GameTimeContext")?.Value,
				Name = element.Element("Name")!.Value,
				ParentId = ulong.Parse(element.Element("Parent")!.Value),
				InheritanceInfo = ParseDemoScalarList(element, "InheritanceInfo"),
				EventIds = ParseDemoListAsUlong(element, "Events").ToArray(),
				ChildObjectIds = ParseDemoListAsUlong(element, "ChildObjects").ToArray(),
				StartEventId = element.Element("StartEvent") != null
					? ulong.Parse(element.Element("StartEvent")!.Value)
					: null
			};

			raws.Add(raw);
		}
	}
}
