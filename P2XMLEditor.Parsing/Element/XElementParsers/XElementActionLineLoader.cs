using System.Xml;
using System.Xml.Linq;
using P2XMLEditor.Enums;
using P2XMLEditor.Enums.VirtualMachine;
using P2XMLEditor.Parsing.Helpers;
using P2XMLEditor.Parsing.RawData;
using static P2XMLEditor.Parsing.Helpers.XElementExtensions;

namespace P2XMLEditor.Parsing.Element.XElementParsers;

public class XElementActionLineLoader : IParser<RawActionLineData> {
	public void ProcessFile(string filePath, List<RawActionLineData> raws) {
		using var xr = XmlReaderExtensions.InitializeFullFileReader(filePath);

		xr.MoveToContent();
		xr.ReadStartElement();
		
		while (xr.NodeType == XmlNodeType.Element) {
			var element = (XElement)XNode.ReadFrom(xr);
			var id = ulong.Parse(element.Attribute(XNameCache.IdAttribute)!.Value);
            
			var loopInfoElement = element.Element(XNameCache.ActionLoopInfo)!;
            
			var raw = new RawActionLineData {
				Id = id,
				ActionIds = ParseListElementAsUlong(element, XNameCache.Actions).ToArray(),
				ActionLineType = element.Element(XNameCache.ActionLineType)!.Value.Deserialize<ActionLineType>(),

				LoopInfoName = loopInfoElement.Element(XNameCache.Name)!.Value,
				LoopInfoStart = loopInfoElement.Element(XNameCache.Start)!.Value,
				LoopInfoEnd = loopInfoElement.Element(XNameCache.End)!.Value,
				LoopInfoRandom = loopInfoElement?.Element(XNameCache.Random)?.Let(ParseBool),

				Name = element.Element(XNameCache.Name)!.Value,
				LocalContextId = ulong.Parse(element.Element(XNameCache.LocalContext)!.Value),
				OrderIndex = int.Parse(element.Element(XNameCache.OrderIndex)!.Value)
			};

			raws.Add(raw);
		}
	}
}