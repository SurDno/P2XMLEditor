using System.Xml.Linq;
using P2XMLEditor.Data;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.Helper;
using static P2XMLEditor.Helper.DemoXmlParsingHelper;

namespace P2XMLEditor.Writing.Element.AlphaXElementWriters;

public class AlphaXElementEntryPointWriter : IAlphaXElementWriter<EntryPoint> {
	public XElement ToXml(EntryPoint element, WriterSettings settings) {
		var obj = new XElement("object");

		if (element.ActionLine != null)
			obj.Add(new XElement("ActionLine", element.ActionLine.Id));
		obj.Add(
			settings.StripNames ? null : CreateDemoStringElement("Name", element.Name),
			new XElement("Parent", element.Parent.Id),
			CreateGuidElement(element.Id)
		);

		return obj;
	}
}
