using System.Xml.Linq;
using P2XMLEditor.Abstract;
using P2XMLEditor.Core;
using P2XMLEditor.Data;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.GameData.VirtualMachineElements.Enums;
using P2XMLEditor.Helper;
using static P2XMLEditor.Helper.XmlParsingHelper;

#pragma warning disable CS8618

namespace P2XMLEditor.GameData.VirtualMachineElements;

public class MindMapNode(string id) : VmElement(id) {
    protected override HashSet<string> KnownElements { get; } = [
        "Name", "Parent", "LogicMapNodeType", "NodeContent", "GameScreenPosX", "GameScreenPosY",
        "InputLinks", "OutputLinks"
    ];

    public string Name { get; set; }
    public MindMap Parent { get; set; }
    public LogicMapNodeType LogicMapNodeType { get; set; }
    public List<MindMapNodeContent> Content { get; set; }
    public List<MindMapLink> InputLinks { get; set; }
    public List<MindMapLink> OutputLinks { get; set; }
    public float GameScreenPosX { get; set; }
    public float GameScreenPosY { get; set; }

    private const float POS_STEP = 0.0025f;

    private record RawMindMapNodeData(string Id, string Name, string ParentId, string LogicMapNodeType, 
        List<string>? ContentIds, List<string>? InputLinkIds, List<string>? OutputLinkIds, float GameScreenPosX,
        float GameScreenPosY) : RawData(Id);

    public override XElement ToXml(WriterSettings settings) {
        var element = CreateBaseElement(Id);
        element.Add(
            new XElement("LogicMapNodeType", LogicMapNodeType.Serialize())
        );
        if (Content.Count != 0)
            element.Add(CreateListElement("NodeContent", Content.Select(c => c.Id)));
        element.Add(
            new XElement("GameScreenPosX", (float)Math.Round(GameScreenPosX / POS_STEP) * POS_STEP),
            new XElement("GameScreenPosY", (float)Math.Round(GameScreenPosY / POS_STEP) * POS_STEP)
        );
        if (InputLinks.Count != 0)
            element.Add(CreateListElement("InputLinks", InputLinks.Select(l => l.Id)));
        if (OutputLinks.Count != 0)
            element.Add(CreateListElement("OutputLinks", OutputLinks.Select(l => l.Id)));
        element.Add(
            new XElement("Name", Name),
            new XElement("Parent", Parent.Id)
        );
        return element;
    }

    protected override RawData CreateRawData(XElement element) {
        return new RawMindMapNodeData(
            element.Attribute("id")?.Value ?? throw new ArgumentException("Id missing"),
            GetRequiredElement(element, "Name").Value,
            GetRequiredElement(element, "Parent").Value,
            GetRequiredElement(element, "LogicMapNodeType").Value,
            ParseListElement(element, "NodeContent"),
            ParseListElement(element, "InputLinks"),
            ParseListElement(element, "OutputLinks"),
            ParseFloat(GetRequiredElement(element, "GameScreenPosX")),
            ParseFloat(GetRequiredElement(element, "GameScreenPosY"))
        );
    }
    
    public override void FillFromRawData(RawData rawData, VirtualMachine vm) {
        if (rawData is not RawMindMapNodeData data)
            throw new ArgumentException($"Expected RawMindMapNodeData but got {rawData.GetType()}");
        
        Name = data.Name;
        Parent = vm.GetElement<MindMap>(data.ParentId);
        LogicMapNodeType = data.LogicMapNodeType.Deserialize<LogicMapNodeType>();
        Content = data.ContentIds?.Select(vm.GetElement<MindMapNodeContent>).ToList() ?? [];
        InputLinks = data.InputLinkIds?.Select(vm.GetElement<MindMapLink>).ToList() ?? [];
        OutputLinks = data.OutputLinkIds?.Select(vm.GetElement<MindMapLink>).ToList() ?? [];
        GameScreenPosX = data.GameScreenPosX;
        GameScreenPosY = data.GameScreenPosY;
    }
    
    protected override VmElement New(VirtualMachine vm, string id, VmElement parent) {
        var node = new MindMapNode(id) {
            Name = "New Node",
            LogicMapNodeType = LogicMapNodeType.Common,
            Content = [],
            InputLinks = [],
            OutputLinks = [],
            Parent = parent as MindMap ?? throw new ArgumentException("MindMapNode parent must be MindMap")
        };
        node.Content.Add(CreateDefault<MindMapNodeContent>(vm, node));
        node.Parent.Nodes.Add(node);
        return node;
    }

    public override void OnDestroy(VirtualMachine vm) {
        Parent.Nodes.Remove(this);
        foreach (var link in InputLinks.Concat(OutputLinks))
            vm.RemoveElement(link);
        foreach (var content in Content)
            vm.RemoveElement(content);
    }
}