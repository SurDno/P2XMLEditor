using P2XMLEditor.GameData.VirtualMachineElements.Enums;
using System.Linq;
using System.Xml.Linq;
using P2XMLEditor.Data;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.Helper;
using static P2XMLEditor.Helper.XmlParsingHelper;

namespace P2XMLEditor.Writing.Element.ReleaseXElementWriters;

public class ReleaseXElementEventWriter : IReleaseXElementWriter<Event> {
	public XElement ToXml(Event element, WriterSettings settings) {
		var xElement = CreateBaseElement(element.Id);
		if (element.EventParameter != null)
			xElement.Add(new XElement("EventParameter", element.EventParameter.Id));
		xElement.Add(new XElement("EventTime",
			$"{element.EventTime.Days}:{element.EventTime.Hours}:{element.EventTime.Minutes}:{element.EventTime.Seconds}"));
		
		if (!settings.RemoveDefaultValueTypes || !element.Manual)
			xElement.Add(CreateBoolElement("Manual", element.Manual));
			
		if (!settings.RemoveDefaultValueTypes || element.EventRaisingType != EventRaisingType.Condition)
			xElement.Add(new XElement("EventRaisingType", element.EventRaisingType.Serialize()));

		if (element.Condition != null)
			xElement.Add(new XElement("Condition", element.Condition.Id));
		if (!settings.RemoveDefaultValueTypes || !element.ChangeTo)
			xElement.Add(CreateBoolElement("ChangeTo", element.ChangeTo));
		if (!settings.RemoveDefaultValueTypes || !element.Repeated)
			xElement.Add(CreateBoolElement("Repeated", element.Repeated));
		if (element.MessagesToWrite.Count > 0) {
			xElement.Add(new XElement("MessagesInfo",
				new XAttribute("count", element.MessagesToWrite.Count),
				element.MessagesToWrite.Select(m => new XElement("Item",
					new XElement("Name", m.Name),
					new XElement("Type", m.Type)
				))
			));
		}
		xElement.Add(
			new XElement("Name", element.Name),
			new XElement("Parent", element.Parent.Id)
		);
		return EnsureFullClosingTag(xElement);
	}
}
