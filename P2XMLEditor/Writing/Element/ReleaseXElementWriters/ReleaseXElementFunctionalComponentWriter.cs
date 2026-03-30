using System.Linq;
using System.Xml.Linq;
using P2XMLEditor.Data;
using P2XMLEditor.GameData.VirtualMachineElements;
using static P2XMLEditor.Helper.XmlParsingHelper;

namespace P2XMLEditor.Writing.Element.ReleaseXElementWriters;

public class ReleaseXElementFunctionalComponentWriter : IReleaseXElementWriter<FunctionalComponent> {
	public XElement ToXml(FunctionalComponent element, WriterSettings settings) {
		var xElement = CreateBaseElement(element.Id);
		xElement.Add(CreateListElement("Events", element.Events.Select(e => e.Id.ToString())));
		if (element.Main != null)
			xElement.Add(CreateBoolElement("Main", (bool)element.Main));
		xElement.Add(
			new XElement("LoadPriority", element.LoadPriority),
			new XElement("Name", element.Name),
			new XElement("Parent", element.Parent.Id)
		);
		return xElement;
	}
}
