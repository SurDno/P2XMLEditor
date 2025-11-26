using System.Xml;
using System.Xml.Linq;
using P2XMLEditor.Core;
using P2XMLEditor.Data;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.GameData.VirtualMachineElements.Enums;
using P2XMLEditor.GameData.VirtualMachineElements.Interfaces;
using P2XMLEditor.Helper;
using P2XMLEditor.Parsing.RawData;
using static P2XMLEditor.Helper.XmlParsingHelper;
using static P2XMLEditor.Helper.XmlReaderExtensions;

#pragma warning disable CS8618

namespace P2XMLEditor.GameData.VirtualMachineElements;

public class Branch(ulong id) : VmElement(id), IGraphElement, IFiller<RawBranchData> {
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
    public bool? IgnoreBlock { get; set; }
    public bool? Initial { get; set; }

    public override XElement ToXml(WriterSettings settings) {
        var element = CreateBaseElement(Id);
        if (BranchConditions.Any())
            element.Add(CreateListElement("BranchConditions", BranchConditions.Select(c => c.Id.ToString())));
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
        element.Add(CreateListElement("EntryPoints", EntryPoints.Select(e => e.Id.ToString())));
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
    
    public override bool IsOrphaned() {
        return Parent.Element switch {
            Graph g => g.States.All(r => r.Element != this),
            Talking t => t.States.All(r => r.Element != this),
            _ => true
        };
    }
    
    public void FillFromRawData(RawBranchData data, VirtualMachine vm) {
        BranchConditions = data.BranchConditionIds?.Select(vm.GetElement<Condition, PartCondition>).ToList() ?? [];
        BranchType = data.BranchType;
        BranchVariantInfo = data.BranchVariantInfo;
        EntryPoints = data.EntryPointIds.Select(vm.GetElement<EntryPoint>).ToList();
        IgnoreBlock = data.IgnoreBlock;
        Owner = vm.GetElement<ParameterHolder>(data.OwnerId);
        InputLinks = data.InputLinkIds?.Select(vm.GetElement<GraphLink>).ToList() ?? [];
        OutputLinks = data.OutputLinkIds?.Select(vm.GetElement<GraphLink>).ToList() ?? [];
        Initial = data.Initial;
        Name = data.Name;
        Parent = vm.GetElement<Graph, Talking>(data.ParentId);
    }
    
    public void OnDestroy(VirtualMachine vm) {
        foreach (var link in InputLinks ?? []) 
            vm.RemoveElement(link);
        foreach (var entryPoint in EntryPoints) 
            vm.RemoveElement(entryPoint);
    }
}

public record struct BranchVariantInfo {
    public string Name { get; set; }
    public string Type { get; set; }
}