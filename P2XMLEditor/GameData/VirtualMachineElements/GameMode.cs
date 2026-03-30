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
    public bool? IsMain { get; set; }
    public TimeSpan StartGameTime { get; set; }
    public float GameTimeSpeed { get; set; }
    public TimeSpan StartSolarTime { get; set; }
    public float SolarTimeSpeed { get; set; }
    public string PlayerRef { get; set; }
    public string Name { get; set; }
    public GameRoot Parent { get; set; }

    
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
    public override void OnDestroy(VirtualMachine vm) {
        vm.First<GameRoot>(_ => true).GameModes.Remove(this);
    }

    public string ParamId => id.ToString();
}