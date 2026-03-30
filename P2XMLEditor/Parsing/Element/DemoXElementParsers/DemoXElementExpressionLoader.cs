using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using P2XMLEditor.Helper;
using P2XMLEditor.Logging;
using P2XMLEditor.Parsing.Helpers;
using P2XMLEditor.Parsing.RawData;
using static P2XMLEditor.Parsing.Helpers.XElementExtensions;

namespace P2XMLEditor.Parsing.Element.DemoXElementParsers;

public class DemoXElementExpressionLoader : IParser<RawExpressionData> {
	
	[PerformanceLogHook]
	public void ProcessFile(string filePath, List<RawExpressionData> raws) {
		using var xr = XmlReaderExtensions.InitializeFullFileReader(filePath);

		xr.MoveToContent();
		xr.ReadStartElement();
		
		while (xr.NodeType == XmlNodeType.Element) {
			var element = (XElement)XNode.ReadFrom(xr);
			var id = ulong.Parse(element.Attribute("id")!.Value);

			var raw = new RawExpressionData {
				Id = id,
				ExpressionType = element.Element("ExpressionType")!.Value,
				TargetFunctionName = element.Element("TargetFunctionName")?.Value ?? string.Empty,
				TargetObject = element.Element("TargetObject")!.Value,
				TargetParam = element.Element("TargetParam")?.Value,
				ConstId = element.Element("Const") != null ?
					ulong.Parse(element.Element("Const")!.Value) : null,
				SourceParams = ParseDemoList(element, "SourceParams").ToArray(),
				LocalContextId = ulong.Parse(element.Element("LocalContext")!.Value),
				Inversion = element.Element("Inversion")?.Let(ParseBool),
				FormulaChilds = ParseDemoListAsUlong(element, "FormulaChilds").ToArray(),
				FormulaOperations = ParseDemoList(element, "FormulaOperations").ToArray()
			};

			raws.Add(raw);
		}
	}
}
