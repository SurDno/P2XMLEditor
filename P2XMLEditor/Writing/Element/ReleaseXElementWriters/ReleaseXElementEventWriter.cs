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
		
		if (element.Manual != null)
			xElement.Add(CreateBoolElement("Manual", (bool)element.Manual));
		xElement.Add(new XElement("EventRaisingType", element.EventRaisingType.Serialize()));
		if (element.Condition != null)
			xElement.Add(new XElement("Condition", element.Condition.Id));
		if (element.ChangeTo != null)
			xElement.Add(CreateBoolElement("ChangeTo", (bool)element.ChangeTo));
		if (element.Repeated != null)
			xElement.Add(CreateBoolElement("Repeated", (bool)element.Repeated));
		if (element.MessagesInfo?.Count > 0) {
			xElement.Add(new XElement("MessagesInfo",
				new XAttribute("count", element.MessagesInfo.Count),
				element.MessagesInfo.Select(m => new XElement("Item",
					new XElement("Name", m.Name),
					new XElement("Type", m.Type)
				))
			));
		}
		xElement.Add(
			new XElement("Name", element.Name),
			new XElement("Parent", element.Parent.Id)
		);
		return xElement;
	}
}
