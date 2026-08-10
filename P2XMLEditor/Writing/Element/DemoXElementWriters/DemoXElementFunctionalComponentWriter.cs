using System.Linq;
using System.Xml.Linq;
using P2XMLEditor.Data;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.Helper;
using static P2XMLEditor.Helper.DemoXmlParsingHelper;

namespace P2XMLEditor.Writing.Element.DemoXElementWriters;

public class DemoXElementFunctionalComponentWriter : IDemoXElementWriter<FunctionalComponent> {
	public XElement ToXml(FunctionalComponent element, WriterSettings settings) {
		var obj = CreateDemoBaseElement(element.Id);

		// The component's events. Without this the demo loader read them (once it too was fixed) and
		// the writer dropped them, so a component came back with none across a save.
		obj.Add(CreateDemoListElementAsLong("Events", element.Events?.Select(e => e.Id) ?? []));

		if (!settings.RemoveDefaultValueTypes || element.Main) obj.Add(CreateDemoBoolElement("Main", element.Main));

		obj.Add(
			CreateDemoStringElement("Name", element.Name),
			new XElement("Parent", element.Parent.Id)
		);

		if (element.LoadPriority != 0)
			obj.Add(new XElement("LoadPriority", element.LoadPriority));

		obj.Add(CreateGuidElement(element.Id));
		return obj;
	}
}
