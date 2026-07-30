using System.Xml.Linq;
using P2XMLEditor.Data;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.Helper;
using static P2XMLEditor.Helper.DemoXmlParsingHelper;

namespace P2XMLEditor.Writing.Element.DemoXElementWriters;

public class DemoXElementReplyWriter : IDemoXElementWriter<Reply> {
	public XElement ToXml(Reply element, WriterSettings settings) {
		var obj = CreateDemoBaseElement(element.Id);

		obj.Add(
			settings.StripNames ? null : CreateDemoStringElement("Name", element.Name),
			new XElement("Text", element.Text.Id)
		);

		if (!settings.RemoveDefaultValueTypes || element.OnlyOnce) obj.Add(CreateDemoBoolElement("OnlyOnce", element.OnlyOnce));

		if (!settings.RemoveDefaultValueTypes || element.OnlyOneReply) obj.Add(CreateDemoBoolElement("OnlyOneReply", element.OnlyOneReply));

		if (!settings.RemoveDefaultValueTypes || element.Default) obj.Add(CreateDemoBoolElement("Default", element.Default));

		if (element.EnableCondition != null)
			obj.Add(new XElement("EnableCondition", element.EnableCondition.Id));
		if (element.ActionLine != null)
			obj.Add(new XElement("ActionLine", element.ActionLine.Id));

		obj.Add(
			new XElement("OrderIndex", element.OrderIndex),
			new XElement("Parent", element.Parent.Id),
			CreateGuidElement(element.Id)
		);

		return obj;
	}
}
