using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.GameData.VirtualMachineElements.Enums;
using P2XMLEditor.Helper;
using P2XMLEditor.Parsing.RawData;
using static P2XMLEditor.Helper.XmlParsingHelper;

namespace P2XMLEditor.Parsing.Element.XElementParsers;

public class XElementActionLineLoader : IParser<RawActionLineData> {
	public void ProcessFile(string filePath, List<RawActionLineData> raws) {
		using var xr = XmlReaderExtensions.InitializeFullFileReader(filePath);

		xr.MoveToContent();
		xr.ReadStartElement();
		
		while (xr.NodeType == XmlNodeType.Element) {
			var element = (XElement)XNode.ReadFrom(xr);
			var id = ulong.Parse(element.Attribute(XNameCache.IdAttribute)!.Value);
            
			ActionLine.ActionLoopInfo? loopInfo = null;
			var loopInfoElement = element.Element(XNameCache.ActionLoopInfo);
			if (loopInfoElement != null) {
				loopInfo = new ActionLine.ActionLoopInfo(
					loopInfoElement.Element(XNameCache.Name)!.Value,
					loopInfoElement.Element(XNameCache.Start)!.Value,
					loopInfoElement.Element(XNameCache.End)!.Value,
					loopInfoElement.Element(XNameCache.Random)?.Let(ParseBool)
				);
			}
            
			var raw = new RawActionLineData {
				Id = id,
				ActionIds = ParseListElementAsUlong(element, XNameCache.Actions),
				ActionLineType = element.Element(XNameCache.ActionLineType)!.Value.Deserialize<ActionLineType>(),
				LoopInfo = loopInfo,
				Name = element.Element(XNameCache.Name)!.Value,
				LocalContextId = ulong.Parse(element.Element(XNameCache.LocalContext)!.Value),
				OrderIndex = int.Parse(element.Element(XNameCache.OrderIndex)!.Value)
			};

			raws.Add(raw);
		}
	}
}