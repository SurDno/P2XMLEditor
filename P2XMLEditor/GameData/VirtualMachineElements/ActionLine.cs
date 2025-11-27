using System.Xml;
using System.Xml.Linq;
using P2XMLEditor.Core;
using P2XMLEditor.Data;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.GameData.VirtualMachineElements.Enums;
using P2XMLEditor.GameData.VirtualMachineElements.Interfaces;
using P2XMLEditor.Helper;
using P2XMLEditor.Parsing.RawData;

using static P2XMLEditor.Helper.XmlReaderExtensions;
using static P2XMLEditor.Helper.XmlParsingHelper;

#pragma warning disable CS8618

namespace P2XMLEditor.GameData.VirtualMachineElements;

public class ActionLine(ulong id) : VmElement(id), IFiller<RawActionLineData> {
    protected override HashSet<string> KnownElements { get; } =
        ["Actions", "ActionLineType", "Name", "LocalContext", "OrderIndex", "ActionLoopInfo"];
    public List<VmEither<Action, ActionLine>>? Actions { get; set; }
    public ActionLineType ActionLineType { get; set; }
    public ActionLoopInfo? LoopInfo { get; set; }
    public string Name { get; set; }
    public VmEither<State, Graph, Branch, Talking, Speech> LocalContext { get; set; }
    public int OrderIndex { get; set; }

    public record struct ActionLoopInfo(string Name, string Start, string End, bool? Random);

    public override XElement ToXml(WriterSettings settings) {
        var element = CreateBaseElement(Id);
        if (Actions is not null && Actions.Count != 0)
            element.Add(CreateListElement("Actions", Actions.Select(a => a.Id.ToString())));
        
        element.Add(new XElement("ActionLineType", ActionLineType.Serialize()));
        
        if (LoopInfo != null) {
            var actionLineInfo = new XElement("ActionLoopInfo",
                CreateSelfClosingElement("Name", LoopInfo.Value.Name),
                new XElement("Start", LoopInfo.Value.Start),
                new XElement("End", LoopInfo.Value.End)
            );
            if (LoopInfo.Value.Random != null)
                actionLineInfo.Add(CreateBoolElement("Random", (bool)LoopInfo.Value.Random!));
            element.Add(actionLineInfo);
        }
        
        element.Add(
            CreateSelfClosingElement("Name", Name),
            new XElement("LocalContext", LocalContext.Id),
            new XElement("OrderIndex", OrderIndex)
        );
        return element;
    }
    
    public void FillFromRawData(RawActionLineData data, VirtualMachine vm) {
        Actions = data.ActionIds?.Select(vm.GetElement<Action, ActionLine>).ToList();
        ActionLineType = data.ActionLineType;
        LoopInfo = data.LoopInfo;
        Name = data.Name;
        LocalContext = vm.GetElement<State, Graph, Branch, Talking, Speech>(data.LocalContextId);
        OrderIndex = data.OrderIndex;
    }
    
    public void OnDestroy(VirtualMachine vm) {
        foreach(var action in Actions ?? [])
            vm.RemoveElement(action.Element);
    }
}
