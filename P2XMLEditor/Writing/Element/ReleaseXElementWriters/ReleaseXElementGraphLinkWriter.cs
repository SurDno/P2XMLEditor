using System.Xml.Linq;
using P2XMLEditor.Data;
using P2XMLEditor.GameData.VirtualMachineElements;
using static P2XMLEditor.Helper.XmlParsingHelper;

namespace P2XMLEditor.Writing.Element.ReleaseXElementWriters;

public class ReleaseXElementGraphLinkWriter : IReleaseXElementWriter<GraphLink> {
	public XElement ToXml(GraphLink element, WriterSettings settings) {
		var xElement = CreateBaseElement(element.Id);
		if (element.Event != null)
			xElement.Add(new XElement("Event", element.Event.Id));
		xElement.Add(
			new XElement("EventObject", element.EventObject?.Write() ?? "%")
		);
		
		if (!settings.RemoveDefaultValueTypes || element.SourceExitPointIndex != -1)
			xElement.Add(new XElement("SourceExitPointIndex", element.SourceExitPointIndex));
		if (!settings.RemoveDefaultValueTypes || element.DestEntryPointIndex != -1)
			xElement.Add(new XElement("DestEntryPointIndex", element.DestEntryPointIndex));

		if (element.SourceParams?.Count > 0)
			xElement.Add(CreateListElement("SourceParams", element.SourceParams));
		if (element.Source != null)
			xElement.Add(new XElement("Source", element.Source.Value.Id));
		if (element.Destination != null)
			xElement.Add(new XElement("Destination", element.Destination.Value.Id));
		if (!settings.RemoveDefaultValueTypes || !element.Enabled)
			xElement.Add(CreateBoolElement("Enabled", element.Enabled));
		if (!settings.StripNames)
			xElement.Add(new XElement("Name", element.Name));
		xElement.Add(
			new XElement("Parent", element.Parent.Id)
		);
		return EnsureFullClosingTag(xElement);
	}
}
