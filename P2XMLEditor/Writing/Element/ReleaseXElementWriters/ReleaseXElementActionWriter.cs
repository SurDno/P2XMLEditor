using System.Xml.Linq;
using P2XMLEditor.Data;
using P2XMLEditor.GameData.VirtualMachineElements.Enums;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.Helper;
using static P2XMLEditor.Helper.XmlParsingHelper;

namespace P2XMLEditor.Writing.Element.ReleaseXElementWriters;

public class ReleaseXElementActionWriter : IReleaseXElementWriter<Action> {
	public XElement ToXml(Action element, WriterSettings settings) {
		var xElement = CreateBaseElement(element.Id);
		
		if (!settings.RemoveDefaultValueTypes || element.ActionType != ActionType.None)
			xElement.Add(new XElement("ActionType", element.ActionType.Serialize()));
		if (!settings.RemoveDefaultValueTypes || element.MathOperationType != MathOperationType.None)
			xElement.Add(new XElement("MathOperationType", element.MathOperationType.Serialize()));

		if(!settings.StripEditorOnlyTags || element.ActionType is ActionType.DoFunction or ActionType.RaiseEvent)
			xElement.Add(CreateSelfClosingElement("TargetFuncName", element.TargetFuncName));
		if (element.SourceExpression != null) 
			xElement.Add(new XElement("SourceExpression", element.SourceExpression.Id));
		if (element.SourceConst != null)
			xElement.Add(new XElement("SourceConst", element.SourceConst));
		xElement.Add(
			new XElement("TargetObject", element.TargetObject),
			new XElement("TargetParam", element.TargetParam)
		);
		if (element.GetParamStrings()?.Count > 0)
			xElement.Add(CreateListElement("SourceParams", element.GetParamStrings()));
		if (!settings.StripNames)
			xElement.Add(CreateSelfClosingElement("Name", element.Name));
		xElement.Add(
			new XElement("LocalContext", element.LocalContext.Id)
		);
		
		if (!settings.RemoveDefaultValueTypes || element.OrderIndex != 0)
			xElement.Add(new XElement("OrderIndex", element.OrderIndex));
		return EnsureFullClosingTag(xElement);
	}
}
