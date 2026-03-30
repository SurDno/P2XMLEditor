using System.Linq;
using System.Xml.Linq;
using P2XMLEditor.Data;
using P2XMLEditor.GameData.VirtualMachineElements;
using static P2XMLEditor.Helper.XmlParsingHelper;

namespace P2XMLEditor.Writing.Element.ReleaseXElementWriters;

public class ReleaseXElementSpeechWriter : IReleaseXElementWriter<Speech> {
    public XElement ToXml(Speech element, WriterSettings settings) {
        var xElement = CreateBaseElement(element.Id);
        if (element.Replies.Any())
            xElement.Add(CreateListElement("Replyes", element.Replies.Select(r => r.Id.ToString())));
        xElement.Add(
            new XElement("Text", element.Text.Id),
            new XElement("AuthorGuid", element.AuthorGuid.Id)
        );
        if (element.OnlyOnce != null)
            xElement.Add(CreateBoolElement("OnlyOnce", (bool)element.OnlyOnce));
        if (element.IsTrade != null)
            xElement.Add(CreateBoolElement("IsTrade", (bool)element.IsTrade));
        if (element.EntryPoints.Any())
            xElement.Add(CreateListElement("EntryPoints", element.EntryPoints.Select(e => e.Id.ToString())) );
        if (element.IgnoreBlock != null)
            xElement.Add(CreateBoolElement("IgnoreBlock", (bool)element.IgnoreBlock));
        xElement.Add(new XElement("Owner", element.Owner.Id));
        if (element.InputLinks?.Any() == true)
            xElement.Add(CreateListElement("InputLinks", element.InputLinks.Select(i => i.Id.ToString())));
        if (element.OutputLinks?.Any() == true)
            xElement.Add(CreateListElement("OutputLinks", element.OutputLinks.Select(o => o.Id.ToString())));
        if (element.Initial != null)
            xElement.Add(CreateBoolElement("Initial", (bool)element.Initial));
        xElement.Add(
            new XElement("Name", element.Name),
            new XElement("Parent", element.Parent.Id)
        );
        return xElement;
    }
}
