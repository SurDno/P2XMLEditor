using System.Xml;
using System.Xml.Linq;
using P2XMLEditor.Core;
using P2XMLEditor.Data;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.GameData.VirtualMachineElements.Enums;
using P2XMLEditor.GameData.VirtualMachineElements.Interfaces;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes;
using P2XMLEditor.Helper;
using P2XMLEditor.Parsing.RawData;
using static P2XMLEditor.Helper.XmlParsingHelper;
using static P2XMLEditor.Helper.XmlReaderExtensions;

#pragma warning disable CS8618

namespace P2XMLEditor.GameData.VirtualMachineElements;

public class Graph(ulong id) : VmElement(id), IFiller<RawGraphData>, IGraphElement {
    protected override HashSet<string> KnownElements { get; } = [
        "States", "EventLinks", "GraphType", "EntryPoints", "IgnoreBlock", "Owner", "InputParamsInfo",
        "InputLinks", "OutputLinks", "Initial", "Name", "Parent", "SubstituteGraph"
    ];
    public List<VmEither<State, Graph, Branch, Talking>> States { get; set; }
    public List<GraphLink> EventLinks { get; set; }
    public GraphType GraphType { get; set; }
    public List<GraphParamInfo>? InputParamsInfo { get; set; }
    public VmEither<ParameterHolder, Graph> Parent { get; set; }
    public VmEither<Graph, Talking>? SubstituteGraph { get; set; }
    
    public List<EntryPoint> EntryPoints { get; set; }
    public List<GraphLink>? InputLinks { get; set; }
    public List<GraphLink>? OutputLinks { get; set; }
    public ParameterHolder Owner { get; set; }
    public string Name { get; set; }
    public bool? IgnoreBlock { get; set; }
    public bool? Initial { get; set; }

    public override XElement ToXml(WriterSettings settings) {
        var element = CreateBaseElement(Id);
        if (SubstituteGraph != null)
            element.Add(new XElement("SubstituteGraph", SubstituteGraph.Id));
        if (States.Any())
            element.Add(CreateListElement("States", States.Select(s => s.Id.ToString())));
        if (EventLinks.Any())
            element.Add(CreateListElement("EventLinks", EventLinks.Select(l => l.Id.ToString())));
        element.Add(new XElement("GraphType", GraphType.Serialize()));
        if (InputParamsInfo?.Any() == true) {
            element.Add(new XElement("InputParamsInfo",
                new XAttribute("count", InputParamsInfo.Count),
                InputParamsInfo.Select(p => new XElement("Item",
                    new XElement("Name", p.Name),
                    new XElement("Type", p.Type)
                ))
            ));
        }
        if (EntryPoints.Any())
            element.Add(CreateListElement("EntryPoints", EntryPoints.Select(l => l.Id.ToString())));
        if (IgnoreBlock != null)
            element.Add(CreateBoolElement("IgnoreBlock", (bool)IgnoreBlock));
        element.Add(new XElement("Owner", Owner.Id));
        if (InputLinks?.Any() == true)
            element.Add(CreateListElement("InputLinks", InputLinks.Select(l => l.Id.ToString())));
        if (OutputLinks?.Any() == true)
            element.Add(CreateListElement("OutputLinks", OutputLinks.Select(l => l.Id.ToString())));
        if (Initial != null)
            element.Add(CreateBoolElement("Initial", (bool)Initial));
        element.Add(
            new XElement("Name", Name),
            new XElement("Parent", Parent.Id)
        );
        return element;
    }
    
    public void FillFromRawData(RawGraphData data, VirtualMachine vm) {
        States = data.StateIds?.Select(vm.GetElement<State, Graph, Branch, Talking>).ToList() ?? [];
        EventLinks = data.EventLinkIds?.Select(vm.GetElement<GraphLink>).ToList() ?? [];
        GraphType = data.GraphType.Deserialize<GraphType>();
        EntryPoints = data.EntryPointIds?.Select(vm.GetElement<EntryPoint>).ToList() ?? [];
        IgnoreBlock = data.IgnoreBlock;
        Owner = vm.GetElement<ParameterHolder>(data.OwnerId);
        InputParamsInfo = data.InputParamsInfo;
        InputParamsInfo?.ForEach(p => p.ResolveReferences(vm));
        InputLinks = data.InputLinkIds?.Select(vm.GetElement<GraphLink>).ToList() ?? [];
        OutputLinks = data.OutputLinkIds?.Select(vm.GetElement<GraphLink>).ToList() ?? [];
        Initial = data.Initial;
        Name = data.Name;
        Parent = vm.GetElement<ParameterHolder, Graph>(data.ParentId);
        SubstituteGraph = data.SubstituteGraphId.HasValue ? 
            vm.GetElement<Graph, Talking>(data.SubstituteGraphId.Value) : null;
    }
    
    public void OnDestroy(VirtualMachine vm) {
        foreach (var state in States) 
            vm.RemoveElement(state.Element);
        foreach (var link in InputLinks ?? []) 
            vm.RemoveElement(link);
        foreach (var entryPoint in EntryPoints) 
            vm.RemoveElement(entryPoint);
        switch (Parent.Element) {
            case ParameterHolder parameterHolder:
                parameterHolder.EventGraph = null;
                break;
            case Graph graph:
                if (graph.SubstituteGraph.Element == this)
                    graph.SubstituteGraph = null;
                graph.States.RemoveAll(s => s.Element == this);
                break;
        }
    }
}