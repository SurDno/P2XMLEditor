using System.Xml.Linq;
using P2XMLEditor.Data;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.Helper;
using static P2XMLEditor.Helper.DemoXmlParsingHelper;

namespace P2XMLEditor.Writing.Element.DemoXElementWriters;

public class DemoXElementGraphLinkWriter : IDemoXElementWriter<GraphLink> {
	public XElement ToXml(GraphLink element, WriterSettings settings) {
		var obj = CreateDemoBaseElement(element.Id);

		obj.Add(
			new XElement("Event", element.Event?.Id ?? 0),
			new XElement("EventObject", element.EventObject.Write()),
			new XElement("Source", element.Source?.Id ?? 0),
			new XElement("Destination", element.Destination?.Id ?? 0),
			new XElement("SourceExitPointIndex", element.SourceExitPointIndex),
			new XElement("DestEntryPointIndex", element.DestEntryPointIndex)
		);

		if (element.Enabled.HasValue)
			obj.Add(CreateDemoBoolElement("Enabled", element.Enabled.Value));

		obj.Add(
			CreateDemoStringElement("Name", element.Name),
			new XElement("Parent", element.Parent.Id),
			CreateGuidElement(element.Id)
		);

		return obj;
	}
}
