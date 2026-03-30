using System.Xml.Linq;
using P2XMLEditor.Data;
using P2XMLEditor.GameData.VirtualMachineElements;
using static P2XMLEditor.Helper.XmlParsingHelper;

namespace P2XMLEditor.Writing.Element.ReleaseXElementWriters;

public class ReleaseXElementCustomTypeWriter : IReleaseXElementWriter<CustomType> {
	public XElement ToXml(CustomType element, WriterSettings settings) {
		var xElement = CreateBaseElement(element.Id);
		xElement.Add(
			new XElement("Name", element.Name),
			new XElement("Parent", element.Parent.Id.ToString())
		);
		return xElement;
	}
}
