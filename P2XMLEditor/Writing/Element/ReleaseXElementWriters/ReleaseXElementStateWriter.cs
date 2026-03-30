using System.Linq;
using System.Xml.Linq;
using P2XMLEditor.Data;
using P2XMLEditor.GameData.VirtualMachineElements;
using static P2XMLEditor.Helper.XmlParsingHelper;

namespace P2XMLEditor.Writing.Element.ReleaseXElementWriters;

public class ReleaseXElementStateWriter : IReleaseXElementWriter<State> {
    public XElement ToXml(State element, WriterSettings settings) {
        var xElement = CreateBaseElement(element.Id);
        xElement.Add(CreateListElement("EntryPoints", element.EntryPoints.Select(a => a.Id.ToString())));
        if (element.IgnoreBlock != null)
            xElement.Add(CreateBoolElement("IgnoreBlock", (bool)element.IgnoreBlock));
        xElement.Add(new XElement("Owner", element.Owner.Id));
        if (element.InputLinks?.Any() == true)
            xElement.Add(CreateListElement("InputLinks", element.InputLinks.Select(a => a.Id.ToString())));
        if (element.OutputLinks?.Any() == true)
            xElement.Add(CreateListElement("OutputLinks", element.OutputLinks.Select(a => a.Id.ToString())));
        if (element.Initial != null)
            xElement.Add(CreateBoolElement("Initial", (bool)element.Initial));
        xElement.Add(
            new XElement("Name", element.Name),
            new XElement("Parent", element.Parent.Id)
        );
        return xElement;
    }
}
