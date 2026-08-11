using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;
using P2XMLEditor.Helper;
using P2XMLEditor.Logging;
using P2XMLEditor.Parsing.Helpers;
using P2XMLEditor.Parsing.RawData;
using static P2XMLEditor.Parsing.Helpers.XElementExtensions;

namespace P2XMLEditor.Parsing.Element.AlphaXElementParsers;

public class AlphaXElementGameModeLoader : IParser<RawGameModeData> {
	[PerformanceLogHook]
	public void ProcessFile(string filePath, List<RawGameModeData> raws) {
		using var xr = AlphaFormat.OpenReader(filePath);

		xr.MoveToContent();
		xr.ReadStartElement();
		
		while (xr.NodeType == XmlNodeType.Element) {
			var element = (XElement)XNode.ReadFrom(xr);
			var id = ulong.Parse(element.Element("Guid")!.Value);

			var raw = new RawGameModeData {
				Id = id,
				IsMain = element.Element("IsMain")?.Let(ParseBool) ?? false,
				StartGameTime = ParseTimeSpanString(element.Element("StartGameTime")!.Value),
				GameTimeSpeed = float.Parse(element.Element("GameTimeSpeed")!.Value),
				StartSolarTime = ParseTimeSpanString(element.Element("StartSolarTime")!.Value),
				SolarTimeSpeed = float.Parse(element.Element("SolarTimeSpeed")!.Value),
				PlayerRef = element.Element("PlayerRef")!.Value,
				Name = element.Element("Name")!.Value,
				ParentId = ulong.Parse(element.Element("Parent")!.Value)
			};

			raws.Add(raw);
		}
	}
}
