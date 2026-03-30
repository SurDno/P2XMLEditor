using System.Linq;
using System.Xml.Linq;
using P2XMLEditor.Data;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.Helper;
using static P2XMLEditor.Helper.DemoXmlParsingHelper;

namespace P2XMLEditor.Writing.Element.DemoXElementWriters;

public class DemoXElementTalkingWriter : IDemoXElementWriter<Talking> {
	public XElement ToXml(Talking element, WriterSettings settings) {
		var obj = CreateDemoBaseElement(element.Id);

		obj.Add(
			CreateDemoListElementAsLong("States", element.States.Select(s => s.Id)),
			CreateDemoListElementAsLong("EventLinks", element.EventLinks.Select(l => l.Id)),
			CreateDemoListElementAsLong("EntryPoints", element.EntryPoints.Select(e => e.Id))
		);

		if (element.IgnoreBlock.HasValue)
			obj.Add(CreateDemoBoolElement("IgnoreBlock", element.IgnoreBlock.Value));

		obj.Add(new XElement("Owner", element.Owner.Id));

		if (element.Initial.HasValue)
			obj.Add(CreateDemoBoolElement("Initial", element.Initial.Value));

		obj.Add(
			CreateDemoStringElement("Name", element.Name),
			new XElement("Parent", element.Parent.Id),
			CreateGuidElement(element.Id)
		);

		return obj;
	}
}
