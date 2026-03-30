using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using P2XMLEditor.Data;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.Helper;
using static P2XMLEditor.Helper.DemoXmlParsingHelper;

namespace P2XMLEditor.Writing.Element.DemoXElementWriters;

public class DemoXElementEventWriter : IDemoXElementWriter<Event> {
	public XElement ToXml(Event element, WriterSettings settings) {
		var obj = CreateDemoBaseElement(element.Id);

		obj.Add(new XElement("EventTime", element.EventTime.ToString("d\\.hh\\:mm\\:ss")));

		if (element.Manual.HasValue)
			obj.Add(CreateDemoBoolElement("Manual", element.Manual.Value));

		obj.Add(new XElement("EventRaisingType", element.EventRaisingType.Serialize()));

		if (element.ChangeTo.HasValue)
			obj.Add(CreateDemoBoolElement("ChangeTo", element.ChangeTo.Value));

		if (element.Repeated.HasValue)
			obj.Add(CreateDemoBoolElement("Repeated", element.Repeated.Value));

		obj.Add(CreateDemoStringElement("Name", element.Name));
		obj.Add(new XElement("Parent", element.Parent.Id));

		if (element.EventParameter != null)
			obj.Add(new XElement("EventParameter", element.EventParameter.Id));
		if (element.Condition != null)
			obj.Add(new XElement("Condition", element.Condition.Id));

		if (element.MessagesInfo?.Count > 0) {
			var serialized = string.Join("MESS&PARAM", element.MessagesInfo.Select(info => {
				var typeWithSuffix = info.Type.Contains('%') ? info.Type : info.Type + "%";
				return $"{info.Name}M&P&PM{typeWithSuffix}M&P&PM{info.Name}";
			}));
			obj.Add(new XElement("MessagesInfo", serialized));
		}

		obj.Add(CreateGuidElement(element.Id));
		return obj;
	}
}
