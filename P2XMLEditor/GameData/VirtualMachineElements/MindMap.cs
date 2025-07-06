using System.Xml.Linq;
using P2XMLEditor.Core;
using P2XMLEditor.Data;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.GameData.VirtualMachineElements.Enums;
using P2XMLEditor.Helper;
using static P2XMLEditor.Helper.XmlParsingHelper;

#pragma warning disable CS8618

namespace P2XMLEditor.GameData.VirtualMachineElements;

public class MindMap(string id) : VmElement(id) {
    protected override HashSet<string> KnownElements { get; } =
        ["Name", "LogicMapType", "Title", "Parent", "Nodes", "Links"];

    public string Name { get; set; }
    public LogicMapType LogicMapType { get; set; }
    public GameString Title { get; set; }
    public GameRoot Parent { get; set; }
    public List<MindMapNode> Nodes { get; set; } = [];
    public List<MindMapLink> Links { get; set; } = [];

    private record RawMindMapData(string Id, string Name, string LogicMapType, string Title, string ParentId,
        List<string> NodeIds, List<string> LinkIds) : RawData(Id);

    public override XElement ToXml(WriterSettings settings) {
        var element = CreateBaseElement(Id);
        if (Nodes.Count != 0)
            element.Add(CreateListElement("Nodes", Nodes.Select(n => n.Id)));
        if (Links.Count != 0)
            element.Add(CreateListElement("Links", Links.Select(l => l.Id)));
        element.Add(
            new XElement("LogicMapType", LogicMapType.Serialize()),
            new XElement("Title", Title.Id),
            new XElement("Name", Name),
            new XElement("Parent", Parent.Id)
        );
        return element;
    }

    protected override RawData CreateRawData(XElement element) {
        return new RawMindMapData(
            element.Attribute("id")?.Value ?? throw new ArgumentException("Id missing"),
            GetRequiredElement(element, "Name").Value,
            GetRequiredElement(element, "LogicMapType").Value,
            GetRequiredElement(element, "Title").Value,
            GetRequiredElement(element, "Parent").Value,
            ParseListElement(element, "Nodes"),
            ParseListElement(element, "Links")
        );
    }

    public override void FillFromRawData(RawData rawData, VirtualMachine vm) {
        if (rawData is not RawMindMapData data)
            throw new ArgumentException($"Expected RawMindMapData but got {rawData.GetType()}");

        Name = data.Name;
        LogicMapType = data.LogicMapType.Deserialize<LogicMapType>();
        Title = vm.GetElement<GameString>(data.Title);
        Parent = vm.GetElement<GameRoot>(data.ParentId);
        Nodes = data.NodeIds.Select(vm.GetElement<MindMapNode>).ToList();
        Links = data.LinkIds.Select(vm.GetElement<MindMapLink>).ToList();
    }
    
    protected override VmElement New(VirtualMachine vm, string id, VmElement parent) {
        var node = new MindMap(id) {
            Name = "New Mind Map",
            LogicMapType = LogicMapType.Global,
            Parent = parent as GameRoot ?? throw new ArgumentException("MindMap parent must be GameRoot")
        };
        node.Title = CreateDefault<GameString>(vm, node.Parent);
        node.Parent.LogicMaps.Add(node);
        return node;
    }

    public override void OnDestroy(VirtualMachine vm) {
        foreach (var node in Nodes)
            vm.RemoveElement(node);
        foreach (var link in Links)
            vm.RemoveElement(link);
        vm.RemoveElement(Title);
        Parent.LogicMaps.Remove(this);
    }
}