using System.Xml.Linq;
using P2XMLEditor.Data;
using P2XMLEditor.GameData.VirtualMachineElements.Enums;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.Helper;
using static P2XMLEditor.Helper.XmlParsingHelper;

namespace P2XMLEditor.Writing.Element.ReleaseXElementWriters;

public class ReleaseXElementMindMapNodeContentWriter : IReleaseXElementWriter<MindMapNodeContent> {
	public XElement ToXml(MindMapNodeContent element, WriterSettings settings) {
		var xElement = CreateBaseElement(element.Id);
		
		if (!settings.RemoveDefaultValueTypes || element.ContentType != NodeContentType.Info)
			xElement.Add(new XElement("ContentType", element.ContentType.Serialize()));
		if (!settings.RemoveDefaultValueTypes || element.Number != 0)
			xElement.Add(new XElement("Number", element.Number));

		xElement.Add(new XElement("ContentDescriptionText", element.ContentDescriptionText.Id));
		
		if (element.ContentPicture != null)
			xElement.Add(new XElement("ContentPicture", element.ContentPicture.Id));
			
		xElement.Add(new XElement("ContentCondition", element.ContentCondition.Id));
		
		if (!settings.StripNames) 
			xElement.Add(CreateSelfClosingElement("Name", element.Name));
		
		if (!settings.StripEditorOnlyTags)
			xElement.Add(new XElement("Parent", element.Parent.Id));
		
		return EnsureFullClosingTag(xElement);
	}
}
