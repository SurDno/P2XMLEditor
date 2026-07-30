using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;
using P2XMLEditor.Helper;
using P2XMLEditor.Logging;
using P2XMLEditor.Parsing.Helpers;
using P2XMLEditor.Parsing.RawData;
using static P2XMLEditor.Parsing.Helpers.XElementExtensions;

namespace P2XMLEditor.Parsing.Element.DemoXElementParsers;

public class DemoXElementGameObjectLoader : IParser<RawGameObjectData> {
	[PerformanceLogHook]
	public void ProcessFile(string filePath, List<RawGameObjectData> raws) {
		using var xr = XmlReaderExtensions.InitializeFullFileReader(filePath);

		xr.MoveToContent();
		xr.ReadStartElement();
		
		while (xr.NodeType == XmlNodeType.Element) {
			var element = (XElement)XNode.ReadFrom(xr);
			var id = ulong.Parse(element.Attribute("id")!.Value);

			var raw = new RawGameObjectData {
				Id = id,
				Static = element.Element("Static")?.Let(ParseBool) ?? false,
				FunctionalComponentIds = ParseDemoListAsUlong(element, "FunctionalComponents").ToArray(),
				EventGraphId = element.Element("EventGraph") != null ?
					ulong.Parse(element.Element("EventGraph")!.Value) : null,
				StandartParamIds = ParseDemoDictAsUlong(element, "StandartParams"),
				CustomParamIds = ParseDemoDictAsUlong(element, "CustomParams"),
				GameTimeContext = element.Element("GameTimeContext")?.Value,
				Name = element.Element("Name")!.Value,
				ParentId = ulong.Parse(element.Element("Parent")!.Value),
				InheritanceInfo = ParseDemoList(element, "InheritanceInfo").ToArray(),
				EventIds = ParseDemoListAsUlong(element, "Events").ToArray(),
				ChildObjectIds = ParseDemoListAsUlong(element, "ChildObjects").ToArray(),
				WorldPositionGuid = element.Element("WorldPositionGuid")?.Value,
				EngineTemplateId = element.Element("EngineTemplateID")?.Value,
				EngineBaseTemplateId = element.Element("EngineBaseTemplateID")?.Value,
				Instantiated = element.Element("Instantiated")?.Let(ParseBool) ?? false
			};

			raws.Add(raw);
		}
	}
}
