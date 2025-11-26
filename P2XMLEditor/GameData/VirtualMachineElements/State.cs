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

public class State(ulong id) : VmElement(id), IFiller<RawStateData>, IGraphElement {
    protected override HashSet<string> KnownElements { get; } = [
        "EntryPoints", "IgnoreBlock", "Owner", "InputLinks", "OutputLinks",
        "Initial", "Name", "Parent"
    ];
    public Graph Parent { get; set; }
    
    public List<EntryPoint> EntryPoints { get; set; }
    public List<GraphLink>? InputLinks { get; set; }
    public List<GraphLink>? OutputLinks { get; set; }
    public ParameterHolder Owner { get; set; }
    public string Name { get; set; }
    public bool? IgnoreBlock { get; set; }
    public bool? Initial { get; set; }

    public override XElement ToXml(WriterSettings settings) {
        var element = CreateBaseElement(Id);
        element.Add(CreateListElement("EntryPoints", EntryPoints.Select(a => a.Id.ToString())));
        if (IgnoreBlock != null)
            element.Add(CreateBoolElement("IgnoreBlock", (bool)IgnoreBlock));
        element.Add(new XElement("Owner", Owner.Id));
        if (InputLinks?.Any() == true)
            element.Add(CreateListElement("InputLinks", InputLinks.Select(a => a.Id.ToString())));
        if (OutputLinks?.Any() == true)
            element.Add(CreateListElement("OutputLinks", OutputLinks.Select(a => a.Id.ToString())));
        if (Initial != null)
            element.Add(CreateBoolElement("Initial", (bool)Initial));
        element.Add(
            new XElement("Name", Name),
            new XElement("Parent", Parent.Id)
        );
        return element;
    }
    
    public void FillFromRawData(RawStateData data, VirtualMachine vm) {
        EntryPoints = data.EntryPointIds.Select(vm.GetElement<EntryPoint>).ToList();
        IgnoreBlock = data.IgnoreBlock;
        Owner = vm.GetElement<ParameterHolder>(data.OwnerId);
        InputLinks = data.InputLinkIds?.Select(vm.GetElement<GraphLink>).ToList();
        OutputLinks = data.OutputLinkIds?.Select(vm.GetElement<GraphLink>).ToList();
        Initial = data.Initial;
        Name = data.Name;
        Parent = vm.GetElement<Graph>(data.ParentId);
    }
    
    public override bool IsOrphaned() => Parent.States.All(r => r.Element != this);
    
    public void OnDestroy(VirtualMachine vm) {
        foreach (var link in InputLinks ?? []) 
            vm.RemoveElement(link);
        foreach (var entryPoint in EntryPoints) 
            vm.RemoveElement(entryPoint);
    }
}