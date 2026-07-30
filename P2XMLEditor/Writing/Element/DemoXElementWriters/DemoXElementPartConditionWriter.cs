using System.Xml.Linq;
using P2XMLEditor.Data;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.Helper;
using static P2XMLEditor.Helper.DemoXmlParsingHelper;

namespace P2XMLEditor.Writing.Element.DemoXElementWriters;

public class DemoXElementPartConditionWriter : IDemoXElementWriter<PartCondition> {
	public XElement ToXml(PartCondition element, WriterSettings settings) {
		var obj = CreateDemoBaseElement(element.Id);

		obj.Add(
			settings.StripNames ? null : CreateDemoStringElement("Name", element.Name),
			new XElement("ConditionType", element.ConditionType)
		);

		if (element.FirstExpression != null)
			obj.Add(new XElement("FirstExpression", element.FirstExpression.Id));
		if (element.SecondExpression != null)
			obj.Add(new XElement("SecondExpression", element.SecondExpression.Id));

		obj.Add(
			new XElement("OrderIndex", element.OrderIndex),
			CreateGuidElement(element.Id)
		);

		return obj;
	}
}
