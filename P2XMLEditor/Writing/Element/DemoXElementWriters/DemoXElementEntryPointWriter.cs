using System.Xml.Linq;
using P2XMLEditor.Data;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.Helper;
using static P2XMLEditor.Helper.DemoXmlParsingHelper;

namespace P2XMLEditor.Writing.Element.DemoXElementWriters;

public class DemoXElementEntryPointWriter : IDemoXElementWriter<EntryPoint> {
	public XElement ToXml(EntryPoint element, WriterSettings settings) {
		var obj = CreateDemoBaseElement(element.Id);

		if (element.ActionLine != null)
			obj.Add(new XElement("ActionLine", element.ActionLine.Id));
		obj.Add(
			CreateDemoStringElement("Name", element.Name),
			new XElement("Parent", element.Parent.Id),
			CreateGuidElement(element.Id)
		);

		return obj;
	}
}
