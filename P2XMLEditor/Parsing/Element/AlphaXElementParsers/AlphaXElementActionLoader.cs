using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using P2XMLEditor.GameData.VirtualMachineElements.Enums;
using P2XMLEditor.Helper;
using P2XMLEditor.Logging;
using P2XMLEditor.Parsing.Helpers;
using P2XMLEditor.Parsing.RawData;

namespace P2XMLEditor.Parsing.Element.AlphaXElementParsers;

public class AlphaXElementActionLoader : IParser<RawActionData> {
	
	[PerformanceLogHook]
	public void ProcessFile(string filePath, List<RawActionData> raws) {
		using var xr = AlphaFormat.OpenReader(filePath);

		xr.MoveToContent();
		xr.ReadStartElement();
		
		while (xr.NodeType == XmlNodeType.Element) {
			var element = (XElement)XNode.ReadFrom(xr);
			var id = ulong.Parse(element.Element("Guid")!.Value);
			
			var raw = new RawActionData {
				Id = id,
				ActionType = element.Element("ActionType")!.Value.Deserialize<ActionType>(),
				MathOperationType = element.Element("MathOperationType")!.Value.Deserialize<MathOperationType>(),
				TargetFuncName = element.Element("TargetFuncName") != null ? element.Element("TargetFuncName")!.Value :
					string.Empty,
				SourceExpressionId = element.Element("SourceExpression") != null
					? ulong.Parse(element.Element("SourceExpression")!.Value)
					: null,
				SourceConstId = element.Element("SourceConst") != null
					? ulong.Parse(element.Element("SourceConst")!.Value)
					: null,
				TargetObject = element.Element("TargetObjUniName")!.Value,
				TargetParam = element.Element("TargetParamName")!.Value,
				SourceParams = ParseDemoList(element, "SourceParamNames").ToArray(),
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
