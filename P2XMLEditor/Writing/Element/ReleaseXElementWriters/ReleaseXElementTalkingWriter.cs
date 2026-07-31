using System.Linq;
using System.Xml.Linq;
using P2XMLEditor.Data;
using P2XMLEditor.GameData.VirtualMachineElements;
using static P2XMLEditor.Helper.XmlParsingHelper;

namespace P2XMLEditor.Writing.Element.ReleaseXElementWriters;

public class ReleaseXElementTalkingWriter : IReleaseXElementWriter<Talking> {
	public XElement ToXml(Talking element, WriterSettings settings) {
		var xElement = CreateBaseElement(element.Id);
		if (element.States.Any())
			xElement.Add(CreateListElement("States", element.States.Select(s => s.Id.ToString())));
		if (element.EventLinks.Any())
			xElement.Add(CreateListElement("EventLinks", element.EventLinks.Select(l => l.Id.ToString())));
		xElement.Add(
			new XElement("GraphType", "GRAPH_TYPE_TALKING"),
			CreateListElement("EntryPoints", element.EntryPoints.Select(e => e.Id.ToString()))
		);
		if (!settings.RemoveDefaultValueTypes || element.IgnoreBlock)
			xElement.Add(CreateBoolElement("IgnoreBlock", element.IgnoreBlock));
		xElement.Add(new XElement("Owner", element.Owner.Id));
		if (element.InputLinks?.Any() == true)
			xElement.Add(CreateListElement("InputLinks", element.InputLinks.Select(l => l.Id.ToString())));
		if (!settings.RemoveDefaultValueTypes || element.Initial)
			xElement.Add(CreateBoolElement("Initial", element.Initial));
		if (!settings.StripNames)
			xElement.Add(new XElement("Name", element.Name));
		xElement.Add(new XElement("Parent", element.Parent.Id));
		return EnsureFullClosingTag(xElement);
	}
}
