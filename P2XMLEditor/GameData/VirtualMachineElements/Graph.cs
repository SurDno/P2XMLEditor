using System.Xml.Linq;
using P2XMLEditor.Core;
using P2XMLEditor.Data;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.GameData.VirtualMachineElements.Enums;
using P2XMLEditor.GameData.VirtualMachineElements.Interfaces;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes;
using P2XMLEditor.Helper;
using static P2XMLEditor.Helper.XmlParsingHelper;

#pragma warning disable CS8618

namespace P2XMLEditor.GameData.VirtualMachineElements;

public class Graph(string id) : VmElement(id), IGraphElement {
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

    private record RawGraphData(string Id, List<string> StateIds, List<string> EventLinkIds, string GraphType,
        List<string> EntryPoints, bool? IgnoreBlock, string Owner, List<GraphParamInfo>? InputParamsInfo, 
        List<string>? InputLinks, List<string>? OutputLinks, bool? Initial, string Name, string ParentId,
        string? SubstituteGraphId) : RawData(Id);

    public override XElement ToXml(WriterSettings settings) {
        var element = CreateBaseElement(Id);
        if (SubstituteGraph != null)
            element.Add(new XElement("SubstituteGraph", SubstituteGraph.Id));
        if (States.Any())
            element.Add(CreateListElement("States", States.Select(s => s.Id)));
        if (EventLinks.Any())
            element.Add(CreateListElement("EventLinks", EventLinks.Select(l => l.Id)));
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
            element.Add(CreateListElement("EntryPoints", EntryPoints.Select(l => l.Id)));
        if (IgnoreBlock != null)
            element.Add(CreateBoolElement("IgnoreBlock", (bool)IgnoreBlock));
        element.Add(new XElement("Owner", Owner.Id));
        if (InputLinks?.Any() == true)
            element.Add(CreateListElement("InputLinks", InputLinks.Select(l => l.Id)));
        if (OutputLinks?.Any() == true)
            element.Add(CreateListElement("OutputLinks", OutputLinks.Select(l => l.Id)));
        if (Initial != null)
            element.Add(CreateBoolElement("Initial", (bool)Initial));
        element.Add(
            new XElement("Name", Name),
            new XElement("Parent", Parent.Id)
        );
        return element;
    }

    protected override RawData CreateRawData(XElement element) {
        var paramInfos = new List<GraphParamInfo>();
        var inputParamsInfo = element.Element("InputParamsInfo");
        if (inputParamsInfo != null) {
            paramInfos.AddRange(inputParamsInfo.Elements("Item").Select(param =>
                new GraphParamInfo(GetRequiredElement(param, "Name").Value, GetRequiredElement(param, "Type").Value)));
        }

        return new RawGraphData(
            element.Attribute("id")?.Value ?? throw new ArgumentException("Id missing"),
            ParseListElement(element, "States"),
            ParseListElement(element, "EventLinks"),
            GetRequiredElement(element, "GraphType").Value,
            ParseListElement(element, "EntryPoints"),
            element.Element("IgnoreBlock")?.Let(ParseBool),
            GetRequiredElement(element, "Owner").Value,
            paramInfos.Count > 0 ? paramInfos : null,
            ParseListElement(element, "InputLinks"),
            ParseListElement(element, "OutputLinks"),
            element.Element("Initial")?.Let(ParseBool),
            GetRequiredElement(element, "Name").Value,
            GetRequiredElement(element, "Parent").Value,
            element.Element("SubstituteGraph")?.Value
        );
    }

    public override void FillFromRawData(RawData rawData, VirtualMachine vm) {
        if (rawData is not RawGraphData data)
            throw new ArgumentException($"Expected RawGraphData but got {rawData.GetType()}");

        States = data.StateIds.Select(vm.GetElement<State, Graph, Branch, Talking>).ToList();
        EventLinks = data.EventLinkIds.Select(vm.GetElement<GraphLink>).ToList();
        GraphType = data.GraphType.Deserialize<GraphType>();
        EntryPoints = data.EntryPoints.Select(vm.GetElement<EntryPoint>).ToList();
        IgnoreBlock = data.IgnoreBlock;
        Owner = vm.GetElement<ParameterHolder>(data.Owner);
        InputParamsInfo = data.InputParamsInfo;
        InputParamsInfo?.ForEach(p => p.ResolveReferences(vm));
        InputLinks = data.InputLinks?.Select(vm.GetElement<GraphLink>).ToList() ?? [];
        OutputLinks = data.OutputLinks?.Select(vm.GetElement<GraphLink>).ToList() ?? [];
        Initial = data.Initial;
        Name = data.Name;
        Parent = vm.GetElement<ParameterHolder, Graph>(data.ParentId);
        SubstituteGraph = data.SubstituteGraphId != null ? vm.GetElement<Graph, Talking>(data.SubstituteGraphId) : null;
    }
    
    protected override VmElement New(VirtualMachine vm, string id, VmElement parent) => throw new NotImplementedException();

    public override void OnDestroy(VirtualMachine vm) {
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