using System.Xml.Linq;
using P2XMLEditor.Data;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.Helper;
using static P2XMLEditor.Helper.DemoXmlParsingHelper;

namespace P2XMLEditor.Writing.Element.DemoXElementWriters;

public class DemoXElementActionWriter : IDemoXElementWriter<Action> {
	public XElement ToXml(Action element, WriterSettings settings) {
		var obj = CreateDemoBaseElement(element.Id);
		
		obj.Add(
			new XElement("ActionType", element.ActionType.Serialize()),
			new XElement("MathOperationType", element.MathOperationType.Serialize()),
			CreateDemoStringElement("TargetFuncName", element.TargetFuncName)
		);

		if (element.SourceExpression != null)
			obj.Add(new XElement("SourceExpression", element.SourceExpression.Id));

		obj.Add(
			new XElement("TargetObject", element.TargetObject),
			new XElement("TargetParam", element.TargetParam)
		);

		obj.Add(CreateDemoListElement("SourceParams", element.SourceParams));

		obj.Add(
			CreateDemoStringElement("Name", element.Name),
			new XElement("LocalContext", element.LocalContext.Id),
			new XElement("OrderIndex", element.OrderIndex)
		);

		if (element.Enabled.HasValue)
			obj.Add(CreateDemoBoolElement("Enabled", element.Enabled.Value));

		obj.Add(CreateGuidElement(element.Id));
		return obj;
	}
}
