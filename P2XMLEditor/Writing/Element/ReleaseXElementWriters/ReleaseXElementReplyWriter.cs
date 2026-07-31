using System.Xml.Linq;
using P2XMLEditor.Data;
using P2XMLEditor.GameData.VirtualMachineElements;
using static P2XMLEditor.Helper.XmlParsingHelper;

namespace P2XMLEditor.Writing.Element.ReleaseXElementWriters;

public class ReleaseXElementReplyWriter : IReleaseXElementWriter<Reply> {
	public XElement ToXml(Reply element, WriterSettings settings) {
		var xElement = CreateBaseElement(element.Id);
		if (!settings.StripNames)
			xElement.Add(new XElement("Name", element.Name));
		xElement.Add(
			new XElement("Text", element.Text.Id)
		);
		if (!settings.RemoveDefaultValueTypes || element.OnlyOnce)
			xElement.Add(CreateBoolElement("OnlyOnce", element.OnlyOnce));
		if (!settings.RemoveDefaultValueTypes || element.OnlyOneReply)
			xElement.Add(CreateBoolElement("OnlyOneReply", element.OnlyOneReply));
		if (!settings.RemoveDefaultValueTypes || element.Default)
			xElement.Add(CreateBoolElement("Default", element.Default));
		if (element.EnableCondition != null)
			xElement.Add(new XElement("EnableCondition", element.EnableCondition.Id));
		if (element.ActionLine != null)
			xElement.Add(new XElement("ActionLine", element.ActionLine.Id));
			
		if (!settings.RemoveDefaultValueTypes || element.OrderIndex != 0)
			xElement.Add(new XElement("OrderIndex", element.OrderIndex));
		if (!settings.StripEditorOnlyTags)
			xElement.Add(new XElement("Parent", element.Parent.Id));
		
		return EnsureFullClosingTag(xElement);
	}
}
