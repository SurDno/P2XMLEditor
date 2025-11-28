using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;
using P2XMLEditor.Helper;
using P2XMLEditor.Parsing.RawData;
using static P2XMLEditor.Helper.XmlParsingHelper;

namespace P2XMLEditor.Parsing.Element.XElementParsers;

public class XElementFunctionalComponentLoader : IParser<RawFunctionalComponentData> {
	public void ProcessFile(string filePath, List<RawFunctionalComponentData> raws) {
		using var xr = XmlReaderExtensions.InitializeFullFileReader(filePath);

		xr.MoveToContent();
		xr.ReadStartElement();
		
		while (xr.NodeType == XmlNodeType.Element) {
			var element = (XElement)XNode.ReadFrom(xr);
			var id = ulong.Parse(element.Attribute(XNameCache.IdAttribute)!.Value);

			var raw = new RawFunctionalComponentData {
				Id = id,
				EventIds = ParseListElementAsUlong(element, XNameCache.Events),
				Main = element.Element(XNameCache.Main)?.Let(ParseBool),
				LoadPriority = long.Parse(element.Element(XNameCache.LoadPriority)!.Value),
				Name = element.Element(XNameCache.Name)!.Value,
				ParentId = ulong.Parse(element.Element(XNameCache.Parent)!.Value)
			};

			raws.Add(raw);
		}
	}
}