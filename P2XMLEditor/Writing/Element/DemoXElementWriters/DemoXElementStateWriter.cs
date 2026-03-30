using System.Linq;
using System.Xml.Linq;
using P2XMLEditor.Data;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.Helper;
using static P2XMLEditor.Helper.DemoXmlParsingHelper;

namespace P2XMLEditor.Writing.Element.DemoXElementWriters;

public class DemoXElementStateWriter : IDemoXElementWriter<State> {
	public XElement ToXml(State element, WriterSettings settings) {
		var obj = CreateDemoBaseElement(element.Id);

		obj.Add(CreateDemoListElementAsLong("EntryPoints", element.EntryPoints.Select(e => e.Id)));

		if (element.IgnoreBlock.HasValue)
			obj.Add(CreateDemoBoolElement("IgnoreBlock", element.IgnoreBlock.Value));

		obj.Add(new XElement("Owner", element.Owner.Id));

		obj.Add(
			CreateDemoListElementAsLong("InputLinks", element.InputLinks?.Select(l => l.Id) ?? []),
			CreateDemoListElementAsLong("OutputLinks", element.OutputLinks?.Select(l => l.Id) ?? [])
		);

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
