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
        if (!settings.CleanUpNames) 
            xElement.Add(CreateSelfClosingElement("Name", element.Name));
        xElement.Add(new XElement("ConditionType", element.ConditionType.Serialize()));
        if (element.ConditionType is not (ConditionType.ConstTrue or ConditionType.ConstFalse) ||
             !settings.CleanUpUnusedProperties) {
            if (element.FirstExpression != null)
                xElement.Add(new XElement("FirstExpression", element.FirstExpression.Id));
            if (element.SecondExpression != null && (element.ConditionType != ConditionType.ValueExpression || 
                                             !settings.CleanUpUnusedProperties))
                xElement.Add(new XElement("SecondExpression", element.SecondExpression.Id));
        }

        xElement.Add(new XElement("OrderIndex", element.OrderIndex));
        return xElement;
    }
}
