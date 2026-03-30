using System.Collections.Generic;
using System.Linq;
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

#pragma warning disable CS8618

namespace P2XMLEditor.GameData.VirtualMachineElements;

public class Graph(ulong id) : VmElement(id), IFiller<RawGraphData>, IGraphElement {
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
    
    public void FillFromRawData(RawGraphData data, VirtualMachine vm) {
        States = data.StateIds?.Select(vm.GetElement<State, Graph, Branch, Talking>).ToList() ?? [];
        EventLinks = data.EventLinkIds?.Select(vm.GetElement<GraphLink>).ToList() ?? [];
        GraphType = data.GraphType.Deserialize<GraphType>();
        EntryPoints = data.EntryPointIds?.Select(vm.GetElement<EntryPoint>).ToList() ?? [];
        IgnoreBlock = data.IgnoreBlock;
        Owner = vm.GetElement<ParameterHolder>(data.OwnerId);
        InputParamsInfo = data.InputParamsInfo?.Select(t => new GraphParamInfo(t.Item1, t.Item2)).ToList();
        InputParamsInfo?.ForEach(p => p.ResolveReferences(vm));
        InputLinks = data.InputLinkIds?.Select(vm.GetElement<GraphLink>).ToList() ?? [];
        OutputLinks = data.OutputLinkIds?.Select(vm.GetElement<GraphLink>).ToList() ?? [];
        Initial = data.Initial;
        Name = data.Name;
        Parent = vm.GetElement<ParameterHolder, Graph>(data.ParentId);
        if (data.SubstituteGraphId.HasValue)
            SubstituteGraph = vm.GetElement<Graph, Talking>(data.SubstituteGraphId.Value);
    }
    
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
                if (graph.SubstituteGraph.HasValue && graph.SubstituteGraph.Value.Element == this)
                    graph.SubstituteGraph = null;
                graph.States.RemoveAll(s => s.Element == this);
                break;
        }
    }
}