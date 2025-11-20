using System.Xml.Linq;
using P2XMLEditor.Core;
using P2XMLEditor.Data;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.GameData.VirtualMachineElements.Enums;
using P2XMLEditor.Helper;
using static P2XMLEditor.Helper.XmlParsingHelper;

#pragma warning disable CS8618

namespace P2XMLEditor.GameData.VirtualMachineElements;

public class MindMapNodeContent(string id) : VmElement(id) {
    protected override HashSet<string> KnownElements { get; } = [
        "Parent", "ContentType", "Number", "ContentDescriptionText", "ContentPicture", "ContentCondition", "Name"
    ];

    public MindMapNode Parent { get; set; }
    public NodeContentType ContentType { get; set; }
    public int Number { get; set; }
    public GameString ContentDescriptionText { get; private set; }

    public Sample? ContentPicture { get; set; }
    public Condition ContentCondition { get; private set; }
    public string Name { get; set; }

    private record RawMindMapNodeContentData(string Id, string ParentId, string ContentType, int Number, 
        string ContentDescriptionText, string? ContentPicture, string ContentCondition, string Name) : RawData(Id);

    public override XElement ToXml(WriterSettings settings) {
        var element = CreateBaseElement(Id);
        element.Add(
            new XElement("ContentType", ContentType.Serialize()),
            new XElement("Number", Number),
            new XElement("ContentDescriptionText", ContentDescriptionText.Id)
        );
        if (ContentPicture != null)
            element.Add(new XElement("ContentPicture", ContentPicture.Id));
        element.Add(
            new XElement("ContentCondition", ContentCondition.Id),
            CreateSelfClosingElement("Name", Name),
            new XElement("Parent", Parent.Id)
        );
        return element;
    }

    protected override RawData CreateRawData(XElement element) {
        return new RawMindMapNodeContentData(
            element.Attribute("id")?.Value ?? throw new ArgumentException("Id missing"),
            GetRequiredElement(element, "Parent").Value,
            GetRequiredElement(element, "ContentType").Value,
            GetRequiredElement(element, "Number").ParseInt(),
            GetRequiredElement(element, "ContentDescriptionText").Value,
            element.Element("ContentPicture")?.Value,
            GetRequiredElement(element, "ContentCondition").Value,
            GetRequiredElement(element, "Name").Value
        );
    }

    public override void FillFromRawData(RawData rawData, VirtualMachine vm) {
        if (rawData is not RawMindMapNodeContentData data)
            throw new ArgumentException($"Expected RawMindMapNodeContentData but got {rawData.GetType()}");
        
        Parent = vm.GetElement<MindMapNode>(data.ParentId);
        ContentType = data.ContentType.Deserialize<NodeContentType>();
        Number = data.Number;
        ContentDescriptionText = vm.GetElement<GameString>(data.ContentDescriptionText);
        ContentPicture = data.ContentPicture != null ? vm.GetElement<Sample>(data.ContentPicture) : null;
        ContentCondition = vm.GetElement<Condition>(data.ContentCondition);
        Name = data.Name;
    }
    
    protected override VmElement New(VirtualMachine vm, string id, VmElement parent) => new MindMapNodeContent(id) {
        Name = "Initial Content",
        ContentType = NodeContentType.Info,
        Parent = parent as MindMapNode ?? throw new ArgumentException("Parent must be MindMapNode"),
        ContentPicture = vm.First<Sample>(s => s.SampleType is SampleType.MindMapPicture),
        ContentDescriptionText = CreateDefault<GameString>(vm, (parent as MindMapNode)!.Parent),
        ContentCondition = CreateDefault<Condition>(vm, null!)
    };

    public override void OnDestroy(VirtualMachine vm) {
        vm.RemoveElement(ContentDescriptionText);
        vm.RemoveElement(ContentCondition);
        Parent.Content.Remove(this);
    }
}