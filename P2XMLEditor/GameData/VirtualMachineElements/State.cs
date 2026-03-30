using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using P2XMLEditor.Core;
using P2XMLEditor.Data;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.GameData.VirtualMachineElements.Interfaces;
using P2XMLEditor.Helper;
using P2XMLEditor.Parsing.RawData;
using static P2XMLEditor.Helper.XmlParsingHelper;

#pragma warning disable CS8618

namespace P2XMLEditor.GameData.VirtualMachineElements;

public class State(ulong id) : VmElement(id), IFiller<RawStateData>, IGraphElement {
    public Graph Parent { get; set; }
    
    public List<EntryPoint> EntryPoints { get; set; }
    public List<GraphLink>? InputLinks { get; set; }
    public List<GraphLink>? OutputLinks { get; set; }
    public ParameterHolder Owner { get; set; }
    public string Name { get; set; }
    public bool? IgnoreBlock { get; set; }
    public bool? Initial { get; set; }
    
    public void FillFromRawData(RawStateData data, VirtualMachine vm) {
        EntryPoints = [];
        foreach (var entrypoint in data.EntryPointIds)
            EntryPoints.Add(vm.GetElement<EntryPoint>(entrypoint));
        IgnoreBlock = data.IgnoreBlock;
        Owner = vm.GetElement<ParameterHolder>(data.OwnerId);
        InputLinks = [];
        if (data.InputLinkIds != null)
          foreach (var entrypoint in data.InputLinkIds)
             InputLinks.Add(vm.GetElement<GraphLink>(entrypoint));
        OutputLinks = [];
        if (data.OutputLinkIds != null)
            foreach (var entrypoint in data.OutputLinkIds)
              OutputLinks.Add(vm.GetElement<GraphLink>(entrypoint));
        Initial = data.Initial;
        Name = data.Name;
        Parent = vm.GetElement<Graph>(data.ParentId);
    }
    
    public override bool IsOrphaned() => Parent.States.All(r => r.Element != this);
    
    public override void OnDestroy(VirtualMachine vm) {
        foreach (var link in InputLinks ?? []) 
            vm.RemoveElement(link);
        foreach (var entryPoint in EntryPoints) 
            vm.RemoveElement(entryPoint);
    }
}