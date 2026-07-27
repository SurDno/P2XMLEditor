using System.Xml.Linq;
using P2XMLEditor.Data;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.Helper;
using static P2XMLEditor.Helper.XmlParsingHelper;

namespace P2XMLEditor.Writing.Element.ReleaseXElementWriters;

public class ReleaseXElementActionWriter : IReleaseXElementWriter<Action> {
	public XElement ToXml(Action element, WriterSettings settings) {
		var xElement = CreateBaseElement(element.Id);
		
		xElement.Add(
			new XElement("ActionType", element.ActionType.Serialize()),
			new XElement("MathOperationType", element.MathOperationType.Serialize()),
			CreateSelfClosingElement("TargetFuncName", element.TargetFuncName)
		);
		if (element.SourceExpression != null) 
			xElement.Add(new XElement("SourceExpression", element.SourceExpression.Id));
		xElement.Add(
			new XElement("TargetObject", element.TargetObject),
			new XElement("TargetParam", element.TargetParam)
		);
		if (element.GetParamStrings()?.Count > 0)
			xElement.Add(CreateListElement("SourceParams", element.GetParamStrings()));
		xElement.Add(
			CreateSelfClosingElement("Name", element.Name),
			new XElement("LocalContext", element.LocalContext.Id),
			new XElement("OrderIndex", element.OrderIndex)
		);
		return xElement;
	}
}
