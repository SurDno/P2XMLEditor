using System.Linq;
using System.Xml.Linq;
using P2XMLEditor.Data;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.Helper;
using static P2XMLEditor.Helper.XmlParsingHelper;

namespace P2XMLEditor.Writing.Element.ReleaseXElementWriters;

public class ReleaseXElementMindMapWriter : IReleaseXElementWriter<MindMap> {
    public XElement ToXml(MindMap element, WriterSettings settings) {
        var xElement = CreateBaseElement(element.Id);
        if (element.Nodes.Count != 0)
            xElement.Add(CreateListElement("Nodes", element.Nodes.Select(n => n.Id.ToString())));
        if (element.Links.Count != 0)
            xElement.Add(CreateListElement("Links", element.Links.Select(l => l.Id.ToString())));
        xElement.Add(
            new XElement("LogicMapType", element.LogicMapType.Serialize()),
            new XElement("Title", element.Title.Id),
            new XElement("Name", element.Name),
            new XElement("Parent", element.Parent.Id)
        );
        return xElement;
    }
}
