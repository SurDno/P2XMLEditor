using System.Xml.Linq;
using P2XMLEditor.Data;
using P2XMLEditor.GameData.VirtualMachineElements;
using static P2XMLEditor.Helper.XmlParsingHelper;

namespace P2XMLEditor.Writing.Element.ReleaseXElementWriters;

public class ReleaseXElementMindMapLinkWriter : IReleaseXElementWriter<MindMapLink> {
	public XElement ToXml(MindMapLink element, WriterSettings settings) {
		var xElement = CreateBaseElement(element.Id);
		xElement.Add(
			new XElement("Source", element.Source.Id),
			new XElement("Destination", element.Destination.Id)
		);
		if (!settings.StripNames)
			xElement.Add(new XElement("Name"));
		xElement.Add(
			new XElement("Parent", element.Parent.Id)
		);
		return EnsureFullClosingTag(xElement);
	}
}
