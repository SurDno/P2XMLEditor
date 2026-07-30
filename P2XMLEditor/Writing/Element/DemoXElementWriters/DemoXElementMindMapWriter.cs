using System.Linq;
using System.Xml.Linq;
using P2XMLEditor.Data;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.Helper;
using static P2XMLEditor.Helper.DemoXmlParsingHelper;

namespace P2XMLEditor.Writing.Element.DemoXElementWriters;

public class DemoXElementMindMapWriter : IDemoXElementWriter<MindMap> {
	public XElement ToXml(MindMap element, WriterSettings settings) {
		var obj = CreateDemoBaseElement(element.Id);

		obj.Add(
			settings.StripNames ? null : CreateDemoStringElement("Name", element.Name),
			new XElement("LogicMapType", element.LogicMapType.Serialize()),
			new XElement("Title", element.Title.Id),
			new XElement("Parent", element.Parent.Id),
			CreateDemoListElementAsLong("Nodes", element.Nodes.Select(n => n.Id)),
			CreateDemoListElementAsLong("Links", element.Links.Select(l => l.Id))
		);

		// DEMO-ONLY
		if (element.TextObjects?.Any() == true) {
			obj.Add(CreateDemoListElementAsLong("TextObjects", element.TextObjects.Select(t => t.Id)));
		}
		
		if (element.ParentFolder.HasValue) {
			obj.Add(new XElement("ParentFolder", element.ParentFolder.Value));
		}

		obj.Add(CreateGuidElement(element.Id));
		return obj;
	}
}
