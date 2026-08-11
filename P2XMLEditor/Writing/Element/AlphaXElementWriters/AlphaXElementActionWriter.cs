using System.Xml.Linq;
using P2XMLEditor.Data;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.Helper;
using static P2XMLEditor.Helper.DemoXmlParsingHelper;

namespace P2XMLEditor.Writing.Element.AlphaXElementWriters;

public class AlphaXElementActionWriter : IAlphaXElementWriter<Action> {
	public XElement ToXml(Action element, WriterSettings settings) {
		var obj = new XElement("object");
		
		obj.Add(
			new XElement("ActionType", element.ActionType.Serialize()),
			new XElement("MathOperationType", element.MathOperationType.Serialize()),
			CreateDemoStringElement("TargetFuncName", element.TargetFuncName)
		);

		if (element.SourceExpression != null)
			obj.Add(new XElement("SourceExpression", element.SourceExpression.Id));

		if (element.SourceConst != null)
			obj.Add(new XElement("SourceConst", element.SourceConst));

		obj.Add(
			new XElement("TargetObjUniName", element.TargetObject),
			new XElement("TargetParamName", element.TargetParam)
		);


		obj.Add(CreateDemoListElement("SourceParamNames", element.GetParamStrings()));
		obj.Add(
			settings.StripNames ? null : CreateDemoStringElement("Name", element.Name),
			new XElement("LocalContext", element.LocalContext.Id),
			new XElement("OrderIndex", element.OrderIndex)
		);

		// TODO: figure out what the default is (likely true) and strip that check to just output "true" when null
		if (element.Enabled.HasValue) obj.Add(CreateDemoBoolElement("Enabled", element.Enabled.Value));

		obj.Add(CreateGuidElement(element.Id));
		return obj;
	}
}
