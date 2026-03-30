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
        if (element.IsMain != null)
            xElement.Add(CreateBoolElement("IsMain", (bool)element.IsMain));
        xElement.Add(
            new XElement("StartGameTime", $"{sgt.Days}:{sgt.Hours}:{sgt.Minutes}:{sgt.Seconds}"),
            new XElement("GameTimeSpeed", element.GameTimeSpeed),
            new XElement("StartSolarTime", $"{sst.Days}:{sst.Hours}:{sst.Minutes}:{sst.Seconds}"),
            new XElement("SolarTimeSpeed", element.SolarTimeSpeed),
            new XElement("PlayerRef", element.PlayerRef),
            new XElement("Name", element.Name),
            new XElement("Parent", element.Parent.Id)
        );
        return xElement;
    }
}
