using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;
using P2XMLEditor.Helper;
using P2XMLEditor.Parsing.RawData;
using static P2XMLEditor.Helper.XmlParsingHelper;

namespace P2XMLEditor.Parsing.Element.XElementParsers;

public class XElementExpressionLoader : IParser<RawExpressionData> {
	public void ProcessFile(string filePath, List<RawExpressionData> raws) {
		using var xr = XmlReaderExtensions.InitializeFullFileReader(filePath);

		xr.MoveToContent();
		xr.ReadStartElement();
		
		while (xr.NodeType == XmlNodeType.Element) {
			var element = (XElement)XNode.ReadFrom(xr);
			var id = ulong.Parse(element.Attribute(XNameCache.IdAttribute)!.Value);

			var raw = new RawExpressionData {
				Id = id,
				ExpressionType = element.Element(XNameCache.ExpressionType)!.Value,
				TargetFunctionName = element.Element(XNameCache.TargetFunctionName)?.Value,
				TargetObject = element.Element(XNameCache.TargetObject)!.Value,
				TargetParam = element.Element(XNameCache.TargetParam)?.Value,
				ConstId = element.Element(XNameCache.Const) != null ?
					ulong.Parse(element.Element(XNameCache.Const)!.Value) : null,
				SourceParams = ParseListElement(element, XNameCache.SourceParams),
				LocalContextId = ulong.Parse(element.Element(XNameCache.LocalContext)!.Value),
				Inversion = element.Element(XNameCache.Inversion)?.Let(ParseBool),
				FormulaChilds = ParseListElementAsUlong(element, XNameCache.FormulaChilds),
				FormulaOperations = ParseListElement(element, XNameCache.FormulaOperations)
			};

			raws.Add(raw);
		}
	}
}