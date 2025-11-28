using System;
using System.Collections.Generic;
using System.Xml.Linq;
using P2XMLEditor.Core;
using P2XMLEditor.Data;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.GameData.VirtualMachineElements.Interfaces;
using P2XMLEditor.Helper;
using P2XMLEditor.Parsing.RawData;
using static P2XMLEditor.Helper.XmlParsingHelper;

namespace P2XMLEditor.GameData.VirtualMachineElements;

public class GameMode(ulong id) : VmElement(id), IFiller<RawGameModeData>, ICommonVariableParameter {
    protected override HashSet<string> KnownElements { get; } = [
        "IsMain", "StartGameTime", "GameTimeSpeed", "StartSolarTime", "SolarTimeSpeed",
        "PlayerRef", "Name", "Parent"
    ];
    
    public bool? IsMain { get; set; }
    public TimeSpan StartGameTime { get; set; }
    public float GameTimeSpeed { get; set; }
    public TimeSpan StartSolarTime { get; set; }
    public float SolarTimeSpeed { get; set; }
    public string PlayerRef { get; set; }
    public string Name { get; set; }
    public GameRoot Parent { get; set; }

    public override XElement ToXml(WriterSettings settings) {
        var element = CreateBaseElement(Id);
        var sgt = StartGameTime;
        var sst = StartSolarTime;
        if (IsMain != null)
            element.Add(CreateBoolElement("IsMain", (bool)IsMain));
        element.Add(
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
    
    public void FillFromRawData(RawGameModeData data, VirtualMachine vm) {
        IsMain = data.IsMain;
        StartGameTime = data.StartGameTime;
        GameTimeSpeed = data.GameTimeSpeed;
        StartSolarTime = data.StartSolarTime;
        SolarTimeSpeed = data.SolarTimeSpeed;
        PlayerRef = data.PlayerRef;
        Name = data.Name;
        Parent = vm.GetElement<GameRoot>(data.ParentId);
    }    
    public void OnDestroy(VirtualMachine vm) {
        vm.First<GameRoot>(_ => true).GameModes.Remove(this);
    }

    public string ParamId => id.ToString();
}