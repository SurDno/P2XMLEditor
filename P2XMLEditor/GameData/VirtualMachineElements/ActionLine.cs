using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using P2XMLEditor.Core;
using P2XMLEditor.Data;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.GameData.VirtualMachineElements.Enums;
using P2XMLEditor.GameData.VirtualMachineElements.Interfaces;
using P2XMLEditor.Helper;
using P2XMLEditor.Parsing.RawData;
using static P2XMLEditor.Helper.XmlParsingHelper;

#pragma warning disable CS8618

namespace P2XMLEditor.GameData.VirtualMachineElements;

public class ActionLine(ulong id) : VmElement(id), IFiller<RawActionLineData> {
    public List<VmEither<Action, ActionLine>>? Actions { get; set; }
    public ActionLineType ActionLineType { get; set; }
    public ActionLoopInfo? LoopInfo { get; set; }
    public string Name { get; set; }
    public VmEither<State, Graph, Branch, Talking, Speech> LocalContext { get; set; }
    public int OrderIndex { get; set; }

    public record struct ActionLoopInfo(string Name, string Start, string End, bool? Random);
    
    public void FillFromRawData(RawActionLineData data, VirtualMachine vm) {
        Actions = [];
        if (data.ActionIds != null)
            foreach (var actionId in data.ActionIds)
                Actions.Add(vm.GetElement<Action,ActionLine>(actionId));
        ActionLineType = data.ActionLineType;
        LoopInfo = new(data.LoopInfoName, data.LoopInfoStart, data.LoopInfoEnd, data.LoopInfoRandom);
        Name = data.Name;
        LocalContext = vm.GetElement<State, Graph, Branch, Talking, Speech>(data.LocalContextId);
        OrderIndex = data.OrderIndex;
    }
    
    public override void OnDestroy(VirtualMachine vm) {
        foreach(var action in Actions ?? [])
            vm.RemoveElement(action.Element);
    }
}
