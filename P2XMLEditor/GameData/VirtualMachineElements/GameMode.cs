using System.Xml.Linq;
using P2XMLEditor.Abstract;
using P2XMLEditor.Core;
using P2XMLEditor.Data;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.GameData.VirtualMachineElements.Interfaces;
using P2XMLEditor.Helper;
using static P2XMLEditor.Helper.XmlParsingHelper;

namespace P2XMLEditor.GameData.VirtualMachineElements;

public class GameMode(string id) : VmElement(id), ICommonVariableParameter {
    protected override HashSet<string> KnownElements { get; } = [
        "IsMain", "StartGameTime", "GameTimeSpeed", "StartSolarTime", "SolarTimeSpeed",
        "PlayerRef", "Name", "Parent"
    ];

    public bool IsMain { get; set; }
    public TimeSpan StartGameTime { get; set; }
    public float GameTimeSpeed { get; set; }
    public TimeSpan StartSolarTime { get; set; }
    public float SolarTimeSpeed { get; set; }
    public string PlayerRef { get; set; }
    public string Name { get; set; }
    public GameRoot Parent { get; set; }

    private record RawGameModeData(string Id, bool IsMain, TimeSpan StartGameTime, float GameTimeSpeed,
        TimeSpan StartSolarTime, float SolarTimeSpeed, string PlayerRef, string Name, string ParentId) : RawData(Id);

    public override XElement ToXml(WriterSettings settings) {
        var element = CreateBaseElement(Id);
        var sgt = StartGameTime;
        var sst = StartSolarTime;
        element.Add(
            CreateBoolElement("IsMain", IsMain),
            new XElement("StartGameTime", $"{sgt.Days}:{sgt.Hours}:{sgt.Minutes}:{sgt.Seconds}"),
            new XElement("GameTimeSpeed", GameTimeSpeed),
            new XElement("StartSolarTime", $"{sst.Days}:{sst.Hours}:{sst.Minutes}:{sst.Seconds}"),
            new XElement("SolarTimeSpeed", SolarTimeSpeed),
            new XElement("PlayerRef", PlayerRef),
            new XElement("Name", Name),
            new XElement("Parent", Parent.Id)
        );
        return element;
    }

    protected override RawData CreateRawData(XElement element) {
        return new RawGameModeData(
            element.Attribute("id")?.Value ?? throw new ArgumentException("Id missing"),
            ParseBool(GetRequiredElement(element, "IsMain")),
            ParseTimeSpan(GetRequiredElement(element, "StartGameTime")),
            ParseFloat(GetRequiredElement(element, "GameTimeSpeed")),
            ParseTimeSpan(GetRequiredElement(element, "StartSolarTime")),
            ParseFloat(GetRequiredElement(element, "SolarTimeSpeed")),
            GetRequiredElement(element, "PlayerRef").Value,
            GetRequiredElement(element, "Name").Value,
            GetRequiredElement(element, "Parent").Value
        );
    }

    public override void FillFromRawData(RawData rawData, VirtualMachine vm) {
        if (rawData is not RawGameModeData data)
            throw new ArgumentException($"Expected RawGameModeData but got {rawData.GetType()}");

        IsMain = data.IsMain;
        StartGameTime = data.StartGameTime;
        GameTimeSpeed = data.GameTimeSpeed;
        StartSolarTime = data.StartSolarTime;
        SolarTimeSpeed = data.SolarTimeSpeed;
        PlayerRef = data.PlayerRef;
        Name = data.Name;
        Parent = vm.GetElement<GameRoot>(data.ParentId);
    }
    
    protected override VmElement New(VirtualMachine vm, string id, VmElement parent) => throw new NotImplementedException();
}