using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;
using P2XMLEditor.GameData.VirtualMachineElements.Enums;
using P2XMLEditor.Helper;
using P2XMLEditor.Logging;
using P2XMLEditor.Parsing.Helpers;
using P2XMLEditor.Parsing.RawData;
using static P2XMLEditor.Parsing.Helpers.XElementExtensions;

namespace P2XMLEditor.Parsing.Element.XElementParsers;

public class XElementActionLineLoader : IParser<RawActionLineData> {
	
	[PerformanceLogHook]
	public void ProcessFile(string filePath, List<RawActionLineData> raws) {
		using var xr = XmlReaderExtensions.InitializeFullFileReader(filePath);

		xr.MoveToContent();
		xr.ReadStartElement();
		
		while (xr.NodeType == XmlNodeType.Element) {
			var element = (XElement)XNode.ReadFrom(xr);
			var id = ulong.Parse(element.Attribute(XNameCache.IdAttribute)!.Value);
			
			var loopInfoElement = element.Element(XNameCache.ActionLoopInfo);
			
			var raw = new RawActionLineData {
				Id = id,
				ActionIds = ParseListElementAsUlong(element, XNameCache.Actions).ToArray(),
				ActionLineType = element.Element(XNameCache.ActionLineType)!.Value.Deserialize<ActionLineType>(),

				LoopInfoName = loopInfoElement?.Element(XNameCache.Name)?.Value ?? null,
				LoopInfoStart = loopInfoElement?.Element(XNameCache.Start)?.Value ?? null,
				LoopInfoEnd = loopInfoElement?.Element(XNameCache.End)?.Value ?? null,
				LoopInfoRandom = loopInfoElement?.Element(XNameCache.Random)?.Let(ParseBool),

				Name = element.Element(XNameCache.Name)!.Value,
				LocalContextId = ulong.Parse(element.Element(XNameCache.LocalContext)!.Value),
				OrderIndex = int.Parse(element.Element(XNameCache.OrderIndex)!.Value)
			};

			raws.Add(raw);
		}
	}
}
