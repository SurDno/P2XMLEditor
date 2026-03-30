using System.Xml.Linq;
using P2XMLEditor.Data;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.Helper;
using static P2XMLEditor.Helper.DemoXmlParsingHelper;

namespace P2XMLEditor.Writing.Element.DemoXElementWriters;

public class DemoXElementGameModeWriter : IDemoXElementWriter<GameMode> {
	public XElement ToXml(GameMode element, WriterSettings settings) {
		var obj = CreateDemoBaseElement(element.Id);

		if (element.IsMain.HasValue)
			obj.Add(CreateDemoBoolElement("IsMain", element.IsMain.Value));

		obj.Add(new XElement("StartGameTime", element.StartGameTime.ToString("d\\.hh\\:mm\\:ss")));
		obj.Add(new XElement("GameTimeSpeed", element.GameTimeSpeed));
		obj.Add(new XElement("StartSolarTime", element.StartSolarTime.ToString("d\\.hh\\:mm\\:ss")));
		obj.Add(new XElement("SolarTimeSpeed", element.SolarTimeSpeed));
		obj.Add(CreateDemoStringElement("PlayerRef", element.PlayerRef));
		obj.Add(CreateDemoStringElement("Name", element.Name));
		obj.Add(new XElement("Parent", element.Parent.Id));

		obj.Add(CreateGuidElement(element.Id));
		return obj;
	}
}
