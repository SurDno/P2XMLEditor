using System.Xml.Linq;
using P2XMLEditor.Data;
using P2XMLEditor.GameData.VirtualMachineElements;
using static P2XMLEditor.Helper.XmlParsingHelper;

namespace P2XMLEditor.Writing.Element.ReleaseXElementWriters;

public class ReleaseXElementGameModeWriter : IReleaseXElementWriter<GameMode> {
	public XElement ToXml(GameMode element, WriterSettings settings) {
		var xElement = CreateBaseElement(element.Id);
		var sgt = element.StartGameTime;
		var sst = element.StartSolarTime;
		if (!settings.RemoveDefaultValueTypes || element.IsMain)
			xElement.Add(CreateBoolElement("IsMain", element.IsMain));
		xElement.Add(
			new XElement("StartGameTime", $"{sgt.Days}:{sgt.Hours}:{sgt.Minutes}:{sgt.Seconds}")
		);
		
		if (!settings.RemoveDefaultValueTypes || element.GameTimeSpeed != 0f)
			xElement.Add(new XElement("GameTimeSpeed", element.GameTimeSpeed));

		xElement.Add(
			new XElement("StartSolarTime", $"{sst.Days}:{sst.Hours}:{sst.Minutes}:{sst.Seconds}")
		);
		
		if (!settings.RemoveDefaultValueTypes || element.SolarTimeSpeed != 0f)
			xElement.Add(new XElement("SolarTimeSpeed", element.SolarTimeSpeed));

		xElement.Add(
			new XElement("PlayerRef", element.PlayerRef),
			new XElement("Name", element.Name));
		
		if (!settings.StripEditorOnlyTags)
			xElement.Add(new XElement("Parent", element.Parent.Id));
		
		return EnsureFullClosingTag(xElement);
	}
}
