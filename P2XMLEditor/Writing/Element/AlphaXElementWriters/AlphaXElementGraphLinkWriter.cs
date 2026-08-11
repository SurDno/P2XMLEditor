using System.Xml.Linq;
using P2XMLEditor.Data;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.Helper;
using static P2XMLEditor.Helper.DemoXmlParsingHelper;

namespace P2XMLEditor.Writing.Element.AlphaXElementWriters;

public class AlphaXElementGraphLinkWriter : IAlphaXElementWriter<GraphLink> {
	public XElement ToXml(GraphLink element, WriterSettings settings) {
		var obj = new XElement("object");

		obj.Add(
			new XElement("Event", element.Event?.Id ?? 0),
			new XElement("EventObjectID", element.EventObject?.Write() ?? "%"),
			new XElement("Source", element.Source?.Id ?? 0),
			new XElement("Destination", element.Destination?.Id ?? 0),
			new XElement("SourceExitPointIndex", element.SourceExitPointIndex),
			new XElement("DestEntryPointIndex", element.DestEntryPointIndex)
		);

		// The arguments the link carries into what it enters. The loader read them; the writer left
		// them out, so a link came back with none — every argument on it lost across a save.
		obj.Add(CreateDemoListElement("SourceParamNames", element.SourceParams ?? []));

		if (!settings.RemoveDefaultValueTypes || !element.Enabled) obj.Add(CreateDemoBoolElement("Enabled", element.Enabled));

		obj.Add(
			settings.StripNames ? null : CreateDemoStringElement("Name", element.Name),
			new XElement("Parent", element.Parent.Id),
			CreateGuidElement(element.Id)
		);

		return obj;
	}
}
