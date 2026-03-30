using System.Linq;
using System.Xml.Linq;
using P2XMLEditor.Data;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.Helper;
using static P2XMLEditor.Helper.XmlParsingHelper;

namespace P2XMLEditor.Writing.Element.ReleaseXElementWriters;

public class ReleaseXElementGraphWriter : IReleaseXElementWriter<Graph> {
    public XElement ToXml(Graph element, WriterSettings settings) {
        var xElement = CreateBaseElement(element.Id);
        if (element.SubstituteGraph != null)
            xElement.Add(new XElement("SubstituteGraph", element.SubstituteGraph.Value.Id));
        if (element.States.Any())
            xElement.Add(CreateListElement("States", element.States.Select(s => s.Id.ToString())));
        if (element.EventLinks.Any())
            xElement.Add(CreateListElement("EventLinks", element.EventLinks.Select(l => l.Id.ToString())));
        xElement.Add(new XElement("GraphType", element.GraphType.Serialize()));
        if (element.InputParamsInfo?.Any() == true) {
            xElement.Add(new XElement("InputParamsInfo",
                new XAttribute("count", element.InputParamsInfo.Count),
                element.InputParamsInfo.Select(p => new XElement("Item",
                    new XElement("Name", p.Name),
                    new XElement("Type", p.Type)
                ))
            ));
        }
        if (element.EntryPoints.Any())
            xElement.Add(CreateListElement("EntryPoints", element.EntryPoints.Select(l => l.Id.ToString())));
        if (element.IgnoreBlock != null)
            xElement.Add(CreateBoolElement("IgnoreBlock", (bool)element.IgnoreBlock));
        xElement.Add(new XElement("Owner", element.Owner.Id));
        if (element.InputLinks?.Any() == true)
            xElement.Add(CreateListElement("InputLinks", element.InputLinks.Select(l => l.Id.ToString())));
        if (element.OutputLinks?.Any() == true)
            xElement.Add(CreateListElement("OutputLinks", element.OutputLinks.Select(l => l.Id.ToString())));
        if (element.Initial != null)
            xElement.Add(CreateBoolElement("Initial", (bool)element.Initial));
        xElement.Add(
            new XElement("Name", element.Name),
            new XElement("Parent", element.Parent.Id)
        );
        return xElement;
    }
}
