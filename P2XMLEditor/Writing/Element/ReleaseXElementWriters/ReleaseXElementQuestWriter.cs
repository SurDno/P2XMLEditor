using System.Xml.Linq;
using P2XMLEditor.Data;
using P2XMLEditor.GameData.VirtualMachineElements;

namespace P2XMLEditor.Writing.Element.ReleaseXElementWriters;

public class ReleaseXElementQuestWriter : ReleaseXElementParameterHolderWriter<Quest> {
	public override XElement ToXml(Quest element, WriterSettings settings) {
		var xElement = base.ToXml(element, settings);
		if (element.StartEvent != null)
			xElement.AddFirst(new XElement("StartEvent", element.StartEvent.Id));
		return xElement;
	}
}
