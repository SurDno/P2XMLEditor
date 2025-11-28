using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;
using P2XMLEditor.Helper;
using P2XMLEditor.Parsing.RawData;

namespace P2XMLEditor.Parsing.Element.XElementParsers;

public class XElementParameterLoader : IParser<RawParameterData> {
	public void ProcessFile(string filePath, List<RawParameterData> raws) {
		using var xr = XmlReaderExtensions.InitializeFullFileReader(filePath);

		xr.MoveToContent();
		xr.ReadStartElement();
		
		while (xr.NodeType == XmlNodeType.Element) {
			var element = (XElement)XNode.ReadFrom(xr);
			var id = ulong.Parse(element.Attribute(XNameCache.IdAttribute)!.Value);

			var raw = new RawParameterData {
				Id = id,
				Name = element.Element(XNameCache.Name)!.Value,
				OwnerComponentId = element.Element(XNameCache.OwnerComponent) != null
					? ulong.Parse(element.Element(XNameCache.OwnerComponent)!.Value)
					: null,
				Type = element.Element(XNameCache.Type)!.Value,
				Value = element.Element(XNameCache.Value)!.Value,
				Implicit = element.Element(XNameCache.Implicit) != null
					? bool.Parse(element.Element(XNameCache.Implicit)!.Value)
					: null,
				ParentId = ulong.Parse(element.Element(XNameCache.Parent)!.Value),
				Custom = element.Element(XNameCache.Custom) != null
					? bool.Parse(element.Element(XNameCache.Custom)!.Value)
					: null
			};

			raws.Add(raw);
		}
	}
}