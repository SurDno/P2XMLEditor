using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using P2XMLEditor.GameData.VirtualMachineElements.Enums;
using P2XMLEditor.Helper;
using P2XMLEditor.Logging;
using P2XMLEditor.Parsing.Helpers;
using P2XMLEditor.Parsing.RawData;

namespace P2XMLEditor.Parsing.Element.DemoXElementParsers;

public class DemoXElementActionLoader : IParser<RawActionData> {
	
	[PerformanceLogHook]
	public void ProcessFile(string filePath, List<RawActionData> raws) {
		using var xr = XmlReaderExtensions.InitializeFullFileReader(filePath);

		xr.MoveToContent();
		xr.ReadStartElement();
		
		while (xr.NodeType == XmlNodeType.Element) {
			var element = (XElement)XNode.ReadFrom(xr);
			var id = ulong.Parse(element.Attribute("id")!.Value);
			
			var raw = new RawActionData {
				Id = id,
				ActionType = element.Element("ActionType")!.Value.Deserialize<ActionType>(),
				MathOperationType = element.Element("MathOperationType")!.Value.Deserialize<MathOperationType>(),
				TargetFuncName = element.Element("TargetFuncName") != null ? element.Element("TargetFuncName")!.Value :
					string.Empty,
				SourceExpressionId = element.Element("SourceExpression") != null
					? ulong.Parse(element.Element("SourceExpression")!.Value)
					: null,
				TargetObject = element.Element("TargetObject")!.Value,
				TargetParam = element.Element("TargetParam")!.Value,
				SourceParams = ParseDemoList(element, "SourceParams").ToArray(),
				Name = element.Element("Name") != null ? element.Element("Name")!.Value : string.Empty,
				LocalContextId = ulong.Parse(element.Element("LocalContext")!.Value),
				OrderIndex = int.Parse(element.Element("OrderIndex")!.Value),
				Enabled = element.Element("Enabled")?.Value == "True"
			};

			raws.Add(raw);
		}
	}

	private List<string> ParseDemoList(XElement element, string name) {
		var listElement = element.Element(name + ".List");
		if (listElement == null || listElement.IsEmpty) return [];
		return listElement.Elements().Select(x => x.Value).ToList();
	}
}
