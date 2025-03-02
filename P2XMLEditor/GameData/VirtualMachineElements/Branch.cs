using System.Xml.Linq;
using P2XMLEditor.Abstract;
using P2XMLEditor.Core;
using P2XMLEditor.Data;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.GameData.VirtualMachineElements.Enums;
using P2XMLEditor.GameData.VirtualMachineElements.Interfaces;
using P2XMLEditor.Helper;
using static P2XMLEditor.Helper.XmlParsingHelper;

#pragma warning disable CS8618

namespace P2XMLEditor.GameData.VirtualMachineElements;

public class Branch(string id) : VmElement(id), IGraphElement {
    protected override HashSet<string> KnownElements { get; } = [
        "BranchConditions", "BranchType", "EntryPoints", "IgnoreBlock", "Owner",
        "InputLinks", "OutputLinks", "Initial", "Name", "Parent", "BranchVariantInfo"
    ];

    public List<VmEither<Condition, PartCondition>> BranchConditions { get; set; }
    public BranchType BranchType { get; set; }
    public List<BranchVariantInfo>? BranchVariantInfo { get; set; }
    public VmEither<Graph, Talking> Parent { get; set; }
    
    public List<EntryPoint> EntryPoints { get; set; }
    public List<GraphLink>? InputLinks { get; set; }
    public List<GraphLink>? OutputLinks { get; set; }
    public ParameterHolder Owner { get; set; }
    public string Name { get; set; }
    public bool IgnoreBlock { get; set; }
    public bool Initial { get; set; }

    private record RawBranchData(string Id, List<string> BranchConditionIds, string BranchType,
        List<BranchVariantInfo>? BranchVariantInfo, List<string> EntryPoints, bool IgnoreBlock, string Owner,
        List<string>? InputLinks, List<string>? OutputLinks, bool Initial, string Name, string ParentId) : RawData(Id);

    public override XElement ToXml(WriterSettings settings) {
        var element = CreateBaseElement(Id);
        if (BranchConditions.Any())
            element.Add(CreateListElement("BranchConditions", BranchConditions.Select(c => c.Id)));
        element.Add(new XElement("BranchType", BranchType.Serialize()));
        
        if (BranchVariantInfo?.Count > 0) {
            var variantInfo = new XElement("BranchVariantInfo");
            variantInfo.Add(new XAttribute("count", BranchVariantInfo.Count));
            variantInfo.Add(BranchVariantInfo.Select(info => 
                new XElement("Item",
                    new XElement("Name", info.Name),
                    new XElement("Type", info.Type)
                )
            ));
            element.Add(variantInfo);
        }
        element.Add(
            CreateListElement("EntryPoints", EntryPoints.Select(e => e.Id)),
            CreateBoolElement("IgnoreBlock", IgnoreBlock),
            new XElement("Owner", Owner.Id)
        );
        if (InputLinks?.Any() == true)
            element.Add(CreateListElement("InputLinks", InputLinks.Select(l => l.Id)));
        if (OutputLinks?.Any() == true)
            element.Add(CreateListElement("OutputLinks", OutputLinks.Select(l => l.Id)));
        element.Add(
            CreateBoolElement("Initial", Initial),
            new XElement("Name", Name),
            new XElement("Parent", Parent.Id)
        );
        return element;
    }

    protected override RawData CreateRawData(XElement element) {
        var branchVariantInfo = element.Element("BranchVariantInfo")?.Elements("Item")
            .Select(item => new BranchVariantInfo {
                Name = item.Element("Name")?.Value ?? string.Empty,
                Type = item.Element("Type")?.Value ?? string.Empty
            })
            .ToList() ?? null;

        return new RawBranchData(
            element.Attribute("id")?.Value ?? throw new ArgumentException("Id missing"),
            ParseListElement(element, "BranchConditions"),
            GetRequiredElement(element, "BranchType").Value,
            branchVariantInfo,
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
        if (rawData is not RawBranchData data)
            throw new ArgumentException($"Expected RawBranchData but got {rawData.GetType()}");

        BranchConditions = data.BranchConditionIds.Select(vm.GetElement<Condition, PartCondition>).ToList();
        BranchType = data.BranchType.Deserialize<BranchType>();
        BranchVariantInfo = data.BranchVariantInfo;
        EntryPoints = data.EntryPoints.Select(vm.GetElement<EntryPoint>).ToList();
        IgnoreBlock = data.IgnoreBlock;
        Owner = vm.GetElement<ParameterHolder>(data.Owner);
        InputLinks = data.InputLinks?.Select(vm.GetElement<GraphLink>).ToList() ?? [];
        OutputLinks = data.OutputLinks?.Select(vm.GetElement<GraphLink>).ToList() ?? [];
        Initial = data.Initial;
        Name = data.Name;
        Parent = vm.GetElement<Graph, Talking>(data.ParentId);
    }
    
    public override bool IsOrphaned() {
        return Parent.Element switch {
            Graph g => g.States.All(r => r.Element != this),
            Talking t => t.States.All(r => r.Element != this),
            _ => true
        };
    }
    
    protected override VmElement New(VirtualMachine vm, string id, VmElement parent) => throw new NotImplementedException();
    
    public override void OnDestroy(VirtualMachine vm) {
        foreach (var link in InputLinks ?? []) 
            vm.RemoveElement(link);
        foreach (var entryPoint in EntryPoints) 
            vm.RemoveElement(entryPoint);
    }
}

public class BranchVariantInfo {
    public string Name { get; set; }
    public string Type { get; set; }
}