using System.Xml.Linq;
using P2XMLEditor.Data;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.Helper;
using static P2XMLEditor.Helper.DemoXmlParsingHelper;

namespace P2XMLEditor.Writing.Element.AlphaXElementWriters;

public class AlphaXElementMindMapNodeContentWriter : IAlphaXElementWriter<MindMapNodeContent> {
	public XElement ToXml(MindMapNodeContent element, WriterSettings settings) {
		var obj = new XElement("object");

		obj.Add(
			new XElement("Parent", element.Parent.Id),
			new XElement("ContentType", element.ContentType.Serialize()),
			new XElement("Number", element.Number),
			new XElement("ContentDescriptionText", element.ContentDescriptionText.Id)
		);

		if (element.ContentPicture != null)
			obj.Add(new XElement("ContentPicture", element.ContentPicture.Id));

		obj.Add(
			new XElement("ContentCondition", element.ContentCondition.Id),
			settings.StripNames ? null : CreateDemoStringElement("Name", element.Name),
			CreateGuidElement(element.Id)
		);

		return obj;
	}
}
