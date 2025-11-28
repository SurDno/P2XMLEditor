using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;
using P2XMLEditor.GameData.VirtualMachineElements.Enums;
using P2XMLEditor.Helper;
using P2XMLEditor.Parsing.RawData;
using static P2XMLEditor.Helper.XmlParsingHelper;

namespace P2XMLEditor.Parsing.Element.XElementParsers;

public class XElementMindMapLoader : IParser<RawMindMapData> {
	public void ProcessFile(string filePath, List<RawMindMapData> raws) {
		using var xr = XmlReaderExtensions.InitializeFullFileReader(filePath);

		xr.MoveToContent();
		xr.ReadStartElement();
		
		while (xr.NodeType == XmlNodeType.Element) {
			var element = (XElement)XNode.ReadFrom(xr);
			var id = ulong.Parse(element.Attribute(XNameCache.IdAttribute)!.Value);

			var raw = new RawMindMapData {
				Id = id,
				Name = element.Element(XNameCache.Name)!.Value,
				LogicMapType = element.Element(XNameCache.LogicMapType)!.Value.Deserialize<LogicMapType>(),
				TitleId = ulong.Parse(element.Element(XNameCache.Title)!.Value),
				ParentId = ulong.Parse(element.Element(XNameCache.Parent)!.Value),
				NodeIds = ParseListElementAsUlong(element, XNameCache.Nodes),
				LinkIds = ParseListElementAsUlong(element, XNameCache.Links)
			};

			raws.Add(raw);
		}
	}
}