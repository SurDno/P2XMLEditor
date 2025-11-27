using System.Xml;
using System.Xml.Linq;
using P2XMLEditor.Core;
using P2XMLEditor.Data;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.GameData.VirtualMachineElements.Interfaces;
using P2XMLEditor.Helper;
using P2XMLEditor.Parsing.RawData;

using static P2XMLEditor.Helper.XmlParsingHelper;
using static P2XMLEditor.Helper.XmlReaderExtensions;

#pragma warning disable CS8618

namespace P2XMLEditor.GameData.VirtualMachineElements;

public class Talking(ulong id) : VmElement(id), IFiller<RawTalkingData>, ICommonVariableParameter {
    protected override HashSet<string> KnownElements { get; } = [
        "States", "EventLinks", "GraphType", "EntryPoints", "IgnoreBlock", "Owner", "Initial", "Name", "Parent"
    ];
    public List<VmEither<Branch, Speech, State>> States { get; set; }
    public List<GraphLink> EventLinks { get; set; }
    public List<EntryPoint> EntryPoints { get; set; }
    public bool? IgnoreBlock { get; set; }
    public VmEither<Blueprint, Character> Owner { get; set; }
    public bool? Initial { get; set; }
    public string Name { get; set; }
    public Graph Parent { get; set; }
    
    public void FillFromRawData(RawTalkingData data, VirtualMachine vm) {
        States = data.StateIds.Select(vm.GetElement<Branch, Speech, State>).ToList();
        EventLinks = data.EventLinkIds.Select(vm.GetElement<GraphLink>).ToList();
        EntryPoints = data.EntryPointIds.Select(vm.GetElement<EntryPoint>).ToList();
        IgnoreBlock = data.IgnoreBlock;
        Owner = vm.GetElement<Blueprint, Character>(data.OwnerId);
        Initial = data.Initial;
        Name = data.Name;
        Parent = vm.GetElement<Graph>(data.ParentId);
    }

    public override XElement ToXml(WriterSettings settings) {
        var element = CreateBaseElement(Id);
        if (States.Any())
            element.Add(CreateListElement("States", States.Select(s => s.Id.ToString())));
        if (EventLinks.Any())
            element.Add(CreateListElement("EventLinks", EventLinks.Select(l => l.Id.ToString())));
        element.Add(
            new XElement("GraphType", "GRAPH_TYPE_TALKING"),
            CreateListElement("EntryPoints", EntryPoints.Select(e => e.Id.ToString()))
        );
        if (IgnoreBlock != null)
            element.Add(CreateBoolElement("IgnoreBlock", (bool)IgnoreBlock));
        element.Add(new XElement("Owner", Owner.Id));
        if (Initial != null)
            element.Add(CreateBoolElement("Initial", (bool)Initial));
        element.Add(
            new XElement("Name", Name),
            new XElement("Parent", Parent.Id)
        );
        return element;
    }
    
    public override bool IsOrphaned() => Parent.States.All(r => r.Element != this);

    public string ParamId => id.ToString();
}