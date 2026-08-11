using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;
using P2XMLEditor.GameData.VirtualMachineElements.Enums;
using P2XMLEditor.Logging;
using P2XMLEditor.Helper;
using P2XMLEditor.Parsing.Helpers;
using P2XMLEditor.Parsing.RawData;
using static P2XMLEditor.Parsing.Helpers.XElementExtensions;

namespace P2XMLEditor.Parsing.Element.AlphaXElementParsers;

public class AlphaXElementActionLineLoader : IParser<RawActionLineData> {
	
	[PerformanceLogHook]
	public void ProcessFile(string filePath, List<RawActionLineData> raws) {
		using var xr = AlphaFormat.OpenReader(filePath);

		xr.MoveToContent();
		xr.ReadStartElement();
		
		while (xr.NodeType == XmlNodeType.Element) {
			var element = (XElement)XNode.ReadFrom(xr);
			var id = ulong.Parse(element.Element("Guid")!.Value);

			var raw = new RawActionLineData {
				Id = id,
				ActionIds = ParseDemoListAsUlong(element, "Actions").ToArray(),
				ActionLineType = element.Element("ActionLineType")!.Value.Deserialize<ActionLineType>(),
				Name = element.Element("Name")?.Value ?? "",
				LocalContextId = ulong.Parse(element.Element("LocalContext")!.Value),
				OrderIndex = int.Parse(element.Element("OrderIndex")!.Value)
			};

			var loopInfo = element.Element("ActionLoopInfo");
			if (loopInfo != null) {
				raw.LoopInfoName = loopInfo.Element("Name")?.Value;
				raw.LoopInfoStart = loopInfo.Element("Start")?.Value;
				raw.LoopInfoEnd = loopInfo.Element("End")?.Value;
				raw.LoopInfoRandom = loopInfo.Element("Random")?.Let(ParseBool) ?? false;
			}

			raws.Add(raw);
		}
	}
}
