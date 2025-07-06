using System.Xml.Linq;
using P2XMLEditor.Core;
using P2XMLEditor.Data;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.GameData.VirtualMachineElements.Enums;
using P2XMLEditor.Helper;
using static P2XMLEditor.Helper.XmlParsingHelper;

#pragma warning disable CS8618

namespace P2XMLEditor.GameData.VirtualMachineElements;

public class ActionLine(string id) : VmElement(id) {
    protected override HashSet<string> KnownElements { get; } =
        ["Actions", "ActionLineType", "Name", "LocalContext", "OrderIndex", "ActionLoopInfo"];

    public List<VmEither<Action, ActionLine>>? Actions { get; set; }
    public ActionLineType ActionLineType { get; set; }
    public ActionLoopInfo? LoopInfo { get; set; }
    public string Name { get; set; }
    public VmEither<State, Graph, Branch, Talking, Speech> LocalContext { get; set; }
    public int OrderIndex { get; set; }

    public record ActionLoopInfo(string Name, string Start, string End, bool? Random);

    private record RawActionLineData(string Id, List<string>? ActionIds, string ActionLineType, 
        ActionLoopInfo? LoopInfo, string Name, string LocalContextId, int OrderIndex) : RawData(Id);

    public override XElement ToXml(WriterSettings settings) {
        var element = CreateBaseElement(Id);
        if (Actions is not null && Actions.Count != 0)
            element.Add(CreateListElement("Actions", Actions.Select(a => a.Id)));
        
        element.Add(new XElement("ActionLineType", ActionLineType.Serialize()));
        
        if (LoopInfo != null) {
            var actionLineInfo = new XElement("ActionLoopInfo",
                CreateSelfClosingElement("Name", LoopInfo.Name),
                new XElement("Start", LoopInfo.Start),
                new XElement("End", LoopInfo.End)
            );
            if (LoopInfo.Random != null)
                actionLineInfo.Add(CreateBoolElement("Random", (bool)LoopInfo.Random!));
            element.Add(actionLineInfo);
        }
        
        element.Add(
            CreateSelfClosingElement("Name", Name),
            new XElement("LocalContext", LocalContext.Id),
            new XElement("OrderIndex", OrderIndex)
        );
        return element;
    }

    protected override RawData CreateRawData(XElement element) {
        ActionLoopInfo? loopInfo = null;
        var loopInfoElement = element.Element("ActionLoopInfo");
        if (loopInfoElement != null) {
            loopInfo = new(
                GetRequiredElement(loopInfoElement, "Name").Value,
                GetRequiredElement(loopInfoElement, "Start").Value,
                GetRequiredElement(loopInfoElement, "End").Value,
                loopInfoElement.Element("Random")?.Let(ParseBool)
            );
        }
        
        return new RawActionLineData(
            element.Attribute("id")?.Value ?? throw new ArgumentException("Id missing"),
            ParseListElement(element, "Actions"),
            GetRequiredElement(element, "ActionLineType").Value,
            loopInfo,
            GetRequiredElement(element, "Name").Value,
            GetRequiredElement(element, "LocalContext").Value,
            ParseInt(GetRequiredElement(element, "OrderIndex"))
        );
    }

    public override void FillFromRawData(RawData rawData, VirtualMachine vm) {
        if (rawData is not RawActionLineData data)
            throw new ArgumentException($"Expected RawActionLineData but got {rawData.GetType()}");

        Actions = data.ActionIds?.Select(vm.GetElement<Action, ActionLine>).ToList();
        ActionLineType = data.ActionLineType.Deserialize<ActionLineType>();

        LoopInfo = data.LoopInfo;
        Name = data.Name;
        LocalContext = vm.GetElement<State, Graph, Branch, Talking, Speech>(data.LocalContextId);
        OrderIndex = data.OrderIndex;
    }
    
    protected override VmElement New(VirtualMachine vm, string id, VmElement parent) => throw new NotImplementedException();
    
    public override void OnDestroy(VirtualMachine vm) {
        foreach(var action in Actions ?? [])
            vm.RemoveElement(action.Element);
    }
}
