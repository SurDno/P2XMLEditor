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

public class AlphaXElementMindMapNodeLoader : IParser<RawMindMapNodeData> {
	[PerformanceLogHook]
	public void ProcessFile(string filePath, List<RawMindMapNodeData> raws) {
		using var xr = XmlReaderExtensions.InitializeFullFileReader(filePath);

		xr.MoveToContent();
		xr.ReadStartElement();
		
		while (xr.NodeType == XmlNodeType.Element) {
			var element = (XElement)XNode.ReadFrom(xr);
			var id = ulong.Parse(element.Element("Guid")!.Value);

			var raw = new RawMindMapNodeData {
				Id = id,
				Name = element.Element("Name")!.Value,
				ParentId = ulong.Parse(element.Element("Parent")!.Value),
				LogicMapNodeType = element.Element("LogicMapNodeType")!.Value.Deserialize<LogicMapNodeType>(),
				ContentIds = ParseDemoListAsUlong(element, "NodeContent").ToArray(),
				InputLinkIds = ParseDemoListAsUlong(element, "InputLinks").ToArray(),
				OutputLinkIds = ParseDemoListAsUlong(element, "OutputLinks").ToArray(),
				GameScreenPosX = element.Element("GameScreenPosX")!.ParseDemoFloat(),
				GameScreenPosY = element.Element("GameScreenPosY")!.ParseDemoFloat(),
				// Demo-only
				Radius = element.Element("Radius")?.ParseDemoFloat(),
				NodeNameTextId = element.Element("NodeNameText") != null ? ulong.Parse(element.Element("NodeNameText")!.Value) : null,
				NodeDescriptionTextId = element.Element("NodeDescriptionText") != null ? ulong.Parse(element.Element("NodeDescriptionText")!.Value) : null,
				GraphPosition = ParseGraphPosition(element.Element("GraphPosition")?.Value),
				Initial = element.Element("Initial")?.Let(ParseBool) ?? false
			};

			raws.Add(raw);
		}
	}

	private (int X, int Y)? ParseGraphPosition(string? value) {
		if (string.IsNullOrEmpty(value)) return null;
		// Format: {X=-297,Y=-120}
		try {
			var parts = value.Trim('{', '}').Split(',');
			var x = int.Parse(parts[0].Split('=')[1]);
			var y = int.Parse(parts[1].Split('=')[1]);
			return (x, y);
		} catch {
			return null;
		}
	}
}
