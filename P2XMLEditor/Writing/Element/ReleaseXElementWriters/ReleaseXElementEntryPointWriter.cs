using System.Xml.Linq;
using P2XMLEditor.Data;
using P2XMLEditor.GameData.VirtualMachineElements;
using static P2XMLEditor.Helper.XmlParsingHelper;

namespace P2XMLEditor.Writing.Element.ReleaseXElementWriters;

public class ReleaseXElementEntryPointWriter : IReleaseXElementWriter<EntryPoint> {
	public XElement ToXml(EntryPoint element, WriterSettings settings) {
		var xElement = CreateBaseElement(element.Id);
		xElement.Add(new XElement("Name", element.Name));
		if (element.ActionLine != null)
			xElement.Add(new XElement("ActionLine", element.ActionLine.Id));
		xElement.Add(new XElement("Parent", element.Parent.Id));
		return xElement;
	}
}
