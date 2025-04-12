using System.Xml.Linq;
using P2XMLEditor.Abstract;
using P2XMLEditor.Core;
using P2XMLEditor.Data;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.GameData.VirtualMachineElements.Interfaces;
using P2XMLEditor.Helper;
using static P2XMLEditor.Helper.XmlParsingHelper;

#pragma warning disable CS8618

namespace P2XMLEditor.GameData.VirtualMachineElements;

public class Talking(string id) : VmElement(id), ICommonVariableParameter {
    protected override HashSet<string> KnownElements { get; } = [
        "States", "EventLinks", "GraphType", "EntryPoints", "IgnoreBlock", "Owner", "Initial", "Name", "Parent"
    ];

    public List<VmEither<Branch, Speech, State>> States { get; set; }
    public List<GraphLink> EventLinks { get; set; }
    public List<EntryPoint> EntryPoints { get; set; }
    public bool IgnoreBlock { get; set; }
    public VmEither<Blueprint, Character> Owner { get; set; }
    public bool Initial { get; set; }
    public string Name { get; set; }
    public Graph Parent { get; set; }

    private record RawTalkingData(string Id, List<string> StateIds, List<string> EventLinkIds,
        List<string> EntryPoints, bool IgnoreBlock, string Owner, bool Initial, string Name,
        string ParentId) : RawData(Id);

    public override XElement ToXml(WriterSettings settings) {
        var element = CreateBaseElement(Id);
        if (States.Any())
            element.Add(CreateListElement("States", States.Select(s => s.Id)));
        if (EventLinks.Any())
            element.Add(CreateListElement("EventLinks", EventLinks.Select(l => l.Id)));
        element.Add(
            new XElement("GraphType", "GRAPH_TYPE_TALKING"),
            CreateListElement("EntryPoints", EntryPoints.Select(e => e.Id)),
            CreateBoolElement("IgnoreBlock", IgnoreBlock),
            new XElement("Owner", Owner.Id),
            CreateBoolElement("Initial", Initial),
            new XElement("Name", Name),
            new XElement("Parent", Parent.Id)
        );
        return element;
    }

    protected override RawData CreateRawData(XElement element) {
        return new RawTalkingData(
            element.Attribute("id")?.Value ?? throw new ArgumentException("Id missing"),
            ParseListElement(element, "States"),
            ParseListElement(element, "EventLinks"),
            ParseListElement(element, "EntryPoints"),
            ParseBool(GetRequiredElement(element, "IgnoreBlock")),
            GetRequiredElement(element, "Owner").Value,
            ParseBool(GetRequiredElement(element, "Initial")),
            GetRequiredElement(element, "Name").Value,
            GetRequiredElement(element, "Parent").Value
        );
    }
    
    public override void FillFromRawData(RawData rawData, VirtualMachine vm) {
        if (rawData is not RawTalkingData data)
            throw new ArgumentException($"Expected RawTalkingData but got {rawData.GetType()}");

        States = data.StateIds.Select(vm.GetElement<Branch, Speech, State>).ToList();
        EventLinks = data.EventLinkIds.Select(vm.GetElement<GraphLink>).ToList();
        EntryPoints = data.EntryPoints.Select(vm.GetElement<EntryPoint>).ToList();
        IgnoreBlock = data.IgnoreBlock;
        Owner = vm.GetElement<Blueprint, Character>(data.Owner);
        Initial = data.Initial;
        Name = data.Name;
        Parent = vm.GetElement<Graph>(data.ParentId);
    }
    
    protected override VmElement New(VirtualMachine vm, string id, VmElement parent) => throw new NotImplementedException();
    
    public override bool IsOrphaned() => Parent.States.All(r => r.Element != this);
}