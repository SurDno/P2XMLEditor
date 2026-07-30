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
