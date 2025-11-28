using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;
using P2XMLEditor.Helper;
using P2XMLEditor.Parsing.RawData;

namespace P2XMLEditor.Parsing.Element.XElementParsers;

public class XElementEntryPointLoader : IParser<RawEntryPointData> {
	public void ProcessFile(string filePath, List<RawEntryPointData> raws) {
		using var xr = XmlReaderExtensions.InitializeFullFileReader(filePath);

		xr.MoveToContent();
		xr.ReadStartElement();
		
		while (xr.NodeType == XmlNodeType.Element) {
			var element = (XElement)XNode.ReadFrom(xr);
			var id = ulong.Parse(element.Attribute(XNameCache.IdAttribute)!.Value);

			var raw = new RawEntryPointData {
				Id = id,
				Name = element.Element(XNameCache.Name)!.Value,
				ActionLineId = element.Element(XNameCache.ActionLine) != null ?
					ulong.Parse(element.Element(XNameCache.ActionLine)!.Value) : null,
				ParentId = ulong.Parse(element.Element(XNameCache.Parent)!.Value)
			};

			raws.Add(raw);
		}
	}
}