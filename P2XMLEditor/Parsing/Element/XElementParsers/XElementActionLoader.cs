using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;
using P2XMLEditor.GameData.VirtualMachineElements.Enums;
using P2XMLEditor.Helper;
using P2XMLEditor.Parsing.RawData;
using static P2XMLEditor.Helper.XmlParsingHelper;

namespace P2XMLEditor.Parsing.Element.XElementParsers;

public class XElementActionLoader : IParser<RawActionData> {
	public void ProcessFile(string filePath, List<RawActionData> raws) {
		using var xr = XmlReaderExtensions.InitializeFullFileReader(filePath);

		xr.MoveToContent();
		xr.ReadStartElement();
		
		while (xr.NodeType == XmlNodeType.Element) {
			var element = (XElement)XNode.ReadFrom(xr);
			var id = ulong.Parse(element.Attribute(XNameCache.IdAttribute)!.Value);
			
			var raw = new RawActionData {
				Id = id,
				ActionType = element.Element(XNameCache.ActionType)!.Value.Deserialize<ActionType>(),
				MathOperationType =
					element.Element(XNameCache.MathOperationType)!.Value.Deserialize<MathOperationType>(),
				TargetFuncName = element.Element(XNameCache.TargetFuncName)!.Value,
				SourceExpressionId = element.Element(XNameCache.SourceExpression) != null
					? ulong.Parse(element.Element(XNameCache.SourceExpression)!.Value)
					: null,
				TargetObject = element.Element(XNameCache.TargetObject)!.Value,
				TargetParam = element.Element(XNameCache.TargetParam)!.Value,
				SourceParams = ParseListElement(element, XNameCache.SourceParams),
				Name = element.Element(XNameCache.Name)!.Value,
				LocalContextId = ulong.Parse(element.Element(XNameCache.LocalContext)!.Value),
				OrderIndex = int.Parse(element.Element(XNameCache.OrderIndex)!.Value)
			};

			raws.Add(raw);
		}
	}
}