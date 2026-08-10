using System.Linq;
using System.Xml.Linq;
using P2XMLEditor.Data;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.Helper;
using static P2XMLEditor.Helper.DemoXmlParsingHelper;

namespace P2XMLEditor.Writing.Element.DemoXElementWriters;

public class DemoXElementMindMapNodeWriter : IDemoXElementWriter<MindMapNode> {
	public XElement ToXml(MindMapNode element, WriterSettings settings) {
		var obj = CreateDemoBaseElement(element.Id);

		obj.Add(
			settings.StripNames ? null : CreateDemoStringElement("Name", element.Name),
			new XElement("Parent", element.Parent.Id),
			new XElement("LogicMapNodeType", element.LogicMapNodeType.Serialize()),
			// NodeContent, not Content: the loader reads "NodeContent.List" — the real files' tag —
			// so a bare "Content.List" was written and then never read back, dropping every node's
			// content on reload.
			CreateDemoListElementAsLong("NodeContent", element.Content.Select(c => c.Id)),
			CreateDemoListElementAsLong("InputLinks", element.InputLinks.Select(l => l.Id)),
			CreateDemoListElementAsLong("OutputLinks", element.OutputLinks.Select(l => l.Id)),
			CreateDemoFloatElement("GameScreenPosX", element.GameScreenPosX),
			CreateDemoFloatElement("GameScreenPosY", element.GameScreenPosY)
		);

		// DEMO-ONLY
		if (element.Radius.HasValue)
			obj.Add(CreateDemoFloatElement("Radius", element.Radius.Value));
		if (element.NodeNameText != null)
			obj.Add(new XElement("NodeNameText", element.NodeNameText.Id));
		if (element.NodeDescriptionText != null)
			obj.Add(new XElement("NodeDescriptionText", element.NodeDescriptionText.Id));
		if (element.GraphPosition.HasValue) {
			obj.Add(new XElement("GraphPosition", $"{element.GraphPosition.Value.X} {element.GraphPosition.Value.Y}"));
		}
		if (element.Initial.HasValue) obj.Add(CreateDemoBoolElement("Initial", element.Initial.Value));

		obj.Add(CreateGuidElement(element.Id));
		return obj;
	}
}
