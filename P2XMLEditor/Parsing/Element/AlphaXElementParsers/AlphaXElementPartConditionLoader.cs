using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;
using P2XMLEditor.Logging;
using P2XMLEditor.Parsing.Helpers;
using P2XMLEditor.Parsing.RawData;

namespace P2XMLEditor.Parsing.Element.AlphaXElementParsers;

public class AlphaXElementPartConditionLoader : IParser<RawPartConditionData> {
	
	[PerformanceLogHook]
	public void ProcessFile(string filePath, List<RawPartConditionData> raws) {
		using var xr = XmlReaderExtensions.InitializeFullFileReader(filePath);

		xr.MoveToContent();
		xr.ReadStartElement();
		
		while (xr.NodeType == XmlNodeType.Element) {
			var element = (XElement)XNode.ReadFrom(xr);
			var id = ulong.Parse(element.Element("Guid")!.Value);

			var raw = new RawPartConditionData {
				Id = id,
				Name = element.Element("Name")?.Value,
				ConditionType = element.Element("ConditionType")!.Value,
				FirstExpressionId = element.Element("FirstExpression") != null
					? ulong.Parse(element.Element("FirstExpression")!.Value)
					: null,
				SecondExpressionId = element.Element("SecondExpression") != null
					? ulong.Parse(element.Element("SecondExpression")!.Value)
					: null,
				OrderIndex = int.Parse(element.Element("OrderIndex")!.Value)
			};

			raws.Add(raw);
		}
	}
}
