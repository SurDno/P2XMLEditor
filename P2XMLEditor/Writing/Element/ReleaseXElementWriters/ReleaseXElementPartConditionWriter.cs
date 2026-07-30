using System.Xml.Linq;
using P2XMLEditor.Data;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.GameData.VirtualMachineElements.Enums;
using P2XMLEditor.Helper;
using static P2XMLEditor.Helper.XmlParsingHelper;

namespace P2XMLEditor.Writing.Element.ReleaseXElementWriters;

public class ReleaseXElementPartConditionWriter : IReleaseXElementWriter<PartCondition> {
	public XElement ToXml(PartCondition element, WriterSettings settings) {
		var xElement = CreateBaseElement(element.Id);
		if (!settings.StripNames) 
			xElement.Add(CreateSelfClosingElement("Name", element.Name));
		
		if (!settings.RemoveDefaultValueTypes || element.ConditionType != ConditionType.ConstFalse)
			xElement.Add(new XElement("ConditionType", element.ConditionType.Serialize()));

		if (element.FirstExpression != null)
			xElement.Add(new XElement("FirstExpression", element.FirstExpression.Id));
		if (element.SecondExpression != null)
			xElement.Add(new XElement("SecondExpression", element.SecondExpression.Id));
		
		if (!settings.RemoveDefaultValueTypes || element.OrderIndex != 0)
			xElement.Add(new XElement("OrderIndex", element.OrderIndex));
		
		return EnsureFullClosingTag(xElement);
	}
}
