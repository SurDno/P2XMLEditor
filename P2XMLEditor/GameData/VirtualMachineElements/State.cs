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

public class State(string id) : VmElement(id), IGraphElement {
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
    public bool IgnoreBlock { get; set; }
    public bool Initial { get; set; }

    private record RawStateData(string Id, List<string> EntryPoints, bool IgnoreBlock, string Owner, 
        List<string>? InputLinks, List<string>? OutputLinks, bool Initial, string Name, string ParentId) : RawData(Id);

    public override XElement ToXml(WriterSettings settings) {
        var element = CreateBaseElement(Id);
        element.Add(CreateListElement("EntryPoints", EntryPoints.Select(a => a.Id)));
        element.Add(
            CreateBoolElement("IgnoreBlock", IgnoreBlock),
            new XElement("Owner", Owner.Id)
        );
        if (InputLinks?.Any() == true)
            element.Add(CreateListElement("InputLinks", InputLinks.Select(a => a.Id)));
        if (OutputLinks?.Any() == true)
            element.Add(CreateListElement("OutputLinks", OutputLinks.Select(a => a.Id)));
        element.Add(
            CreateBoolElement("Initial", Initial),
            new XElement("Name", Name),
            new XElement("Parent", Parent.Id)
        );
        return element;
    }

    protected override RawData CreateRawData(XElement element) {
        return new RawStateData(
            element.Attribute("id")?.Value ?? throw new ArgumentException("Id missing"),
            ParseListElement(element, "EntryPoints"),
            ParseBool(GetRequiredElement(element, "IgnoreBlock")),
            GetRequiredElement(element, "Owner").Value,
            ParseListElement(element, "InputLinks"),
            ParseListElement(element, "OutputLinks"),
            ParseBool(GetRequiredElement(element, "Initial")),
            GetRequiredElement(element, "Name").Value,
            GetRequiredElement(element, "Parent").Value
        );
    }

    public override void FillFromRawData(RawData rawData, VirtualMachine vm) {
        if (rawData is not RawStateData data)
            throw new ArgumentException($"Expected RawStateData but got {rawData.GetType()}");

        EntryPoints = data.EntryPoints.Select(vm.GetElement<EntryPoint>).ToList();
        IgnoreBlock = data.IgnoreBlock;
        Owner = vm.GetElement<ParameterHolder>(data.Owner);
        InputLinks = data.InputLinks?.Select(vm.GetElement<GraphLink>).ToList();
        OutputLinks =  data.OutputLinks?.Select(vm.GetElement<GraphLink>).ToList();
        Initial = data.Initial;
        Name = data.Name;
        Parent = vm.GetElement<Graph>(data.ParentId);
    }
    
    protected override VmElement New(VirtualMachine vm, string id, VmElement parent) => throw new NotImplementedException();
    
    public override bool IsOrphaned() => Parent.States.All(r => r.Element != this);
    
    public override void OnDestroy(VirtualMachine vm) {
        foreach (var link in InputLinks ?? []) 
            vm.RemoveElement(link);
        foreach (var entryPoint in EntryPoints) 
            vm.RemoveElement(entryPoint);
    }
}