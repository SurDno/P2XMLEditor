using System.Xml.Linq;
using P2XMLEditor.Data;
using P2XMLEditor.GameData.VirtualMachineElements;
using static P2XMLEditor.Helper.XmlParsingHelper;

namespace P2XMLEditor.Writing.Element.ReleaseXElementWriters;

public class ReleaseXElementReplyWriter : IReleaseXElementWriter<Reply> {
    public XElement ToXml(Reply element, WriterSettings settings) {
        var xElement = CreateBaseElement(element.Id);
        xElement.Add(
            new XElement("Name", element.Name),
            new XElement("Text", element.Text.Id)
        );
        if (element.OnlyOnce != null)
            xElement.Add(CreateBoolElement("OnlyOnce", (bool)element.OnlyOnce));
        if (element.OnlyOneReply != null)
            xElement.Add(CreateBoolElement("OnlyOneReply", (bool)element.OnlyOneReply));
        if (element.Default != null)
            xElement.Add(CreateBoolElement("Default", (bool)element.Default));
        if (element.EnableCondition != null)
            xElement.Add(new XElement("EnableCondition", element.EnableCondition.Id));
        if (element.ActionLine != null)
            xElement.Add(new XElement("ActionLine", element.ActionLine.Id));
        xElement.Add(
            new XElement("OrderIndex", element.OrderIndex),
            new XElement("Parent", element.Parent.Id)
        );
        return xElement;
    }
}
