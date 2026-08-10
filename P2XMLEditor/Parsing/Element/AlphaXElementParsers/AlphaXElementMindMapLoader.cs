using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using P2XMLEditor.GameData.VirtualMachineElements.Enums;
using P2XMLEditor.Helper;
using P2XMLEditor.Logging;
using P2XMLEditor.Parsing.Helpers;
using P2XMLEditor.Parsing.RawData;
using static P2XMLEditor.Parsing.Helpers.XElementExtensions;

namespace P2XMLEditor.Parsing.Element.AlphaXElementParsers;

public class AlphaXElementMindMapLoader : IParser<RawMindMapData> {
	[PerformanceLogHook]
	public void ProcessFile(string filePath, List<RawMindMapData> raws) {
		using var xr = XmlReaderExtensions.InitializeFullFileReader(filePath);

		xr.MoveToContent();
		xr.ReadStartElement();
		
		while (xr.NodeType == XmlNodeType.Element) {
			var element = (XElement)XNode.ReadFrom(xr);
			var id = ulong.Parse(element.Element("Guid")!.Value);

			var raw = new RawMindMapData {
				Id = id,
				Name = element.Element("Name")!.Value,
				LogicMapType = element.Element("LogicMapType")!.Value.Deserialize<LogicMapType>(),
				TitleId = ulong.Parse(element.Element("Title")!.Value),
				ParentId = ulong.Parse(element.Element("Parent")!.Value),
				NodeIds = ParseDemoListAsUlong(element, "Nodes").ToArray(),
				LinkIds = ParseDemoListAsUlong(element, "Links").ToArray(),
				// Demo-only
				TextObjectIds = ParseDemoListAsUlong(element, "TextObjects").ToArray(),
				ParentFolder = element.Element("ParentFolder") != null ? ulong.Parse(element.Element("ParentFolder")!.Value) : null
			};

			raws.Add(raw);
		}
	}
}
