using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;
using P2XMLEditor.Helper;
using P2XMLEditor.Parsing.RawData;

namespace P2XMLEditor.Parsing.Element.XElementParsers;

public class XElementPartConditionLoader : IParser<RawPartConditionData> {
	public void ProcessFile(string filePath, List<RawPartConditionData> raws) {
		using var xr = XmlReaderExtensions.InitializeFullFileReader(filePath);

		xr.MoveToContent();
		xr.ReadStartElement();
		
		while (xr.NodeType == XmlNodeType.Element) {
			var element = (XElement)XNode.ReadFrom(xr);
			var id = ulong.Parse(element.Attribute(XNameCache.IdAttribute)!.Value);

			var raw = new RawPartConditionData {
				Id = id,
				Name = element.Element(XNameCache.Name)?.Value,
				ConditionType = element.Element(XNameCache.ConditionType)!.Value,
				FirstExpressionId = element.Element(XNameCache.FirstExpression) != null
					? ulong.Parse(element.Element(XNameCache.FirstExpression)!.Value)
					: null,
				SecondExpressionId = element.Element(XNameCache.SecondExpression) != null
					? ulong.Parse(element.Element(XNameCache.SecondExpression)!.Value)
					: null,
				OrderIndex = int.Parse(element.Element(XNameCache.OrderIndex)!.Value)
			};

			raws.Add(raw);
		}
	}
}