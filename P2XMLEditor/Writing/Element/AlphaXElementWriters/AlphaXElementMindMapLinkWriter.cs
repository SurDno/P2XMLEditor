using System.Xml.Linq;
using P2XMLEditor.Data;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.Helper;
using static P2XMLEditor.Helper.DemoXmlParsingHelper;

namespace P2XMLEditor.Writing.Element.AlphaXElementWriters;

public class AlphaXElementMindMapLinkWriter : IAlphaXElementWriter<MindMapLink> {
	public XElement ToXml(MindMapLink element, WriterSettings settings) {
		var obj = new XElement("object");

		obj.Add(
			new XElement("Source", element.Source.Id),
			new XElement("Destination", element.Destination.Id),
			new XElement("Enabled", "True"),
			new XElement("Parent", element.Parent.Id),
			CreateGuidElement(element.Id)
		);

		return obj;
	}
}
