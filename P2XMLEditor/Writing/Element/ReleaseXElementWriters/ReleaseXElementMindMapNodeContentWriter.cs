using System.Xml.Linq;
using P2XMLEditor.Data;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.Helper;
using static P2XMLEditor.Helper.XmlParsingHelper;

namespace P2XMLEditor.Writing.Element.ReleaseXElementWriters;

public class ReleaseXElementMindMapNodeContentWriter : IReleaseXElementWriter<MindMapNodeContent> {
    public XElement ToXml(MindMapNodeContent element, WriterSettings settings) {
        var xElement = CreateBaseElement(element.Id);
        xElement.Add(
            new XElement("ContentType", element.ContentType.Serialize()),
            new XElement("Number", element.Number),
            new XElement("ContentDescriptionText", element.ContentDescriptionText.Id)
        );
        if (element.ContentPicture != null)
            xElement.Add(new XElement("ContentPicture", element.ContentPicture.Id));
        xElement.Add(
            new XElement("ContentCondition", element.ContentCondition.Id),
            CreateSelfClosingElement("Name", element.Name),
            new XElement("Parent", element.Parent.Id)
        );
        return xElement;
    }
}
