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

public class Speech(string id) : VmElement(id) {
    protected override HashSet<string> KnownElements { get; } = [
        "Replyes", "Text", "AuthorGuid", "OnlyOnce", "IsTrade", "EntryPoints",
        "IgnoreBlock", "Owner", "InputLinks", "OutputLinks", "Initial", "Name", "Parent"
    ];

    public List<Reply> Replies { get; set; }
    public GameString Text { get; set; }
    public VmEither<Blueprint, Character> AuthorGuid { get; set; }
    public bool OnlyOnce { get; set; }
    public bool IsTrade { get; set; }
    public List<EntryPoint> EntryPoints { get; set; }
    public bool IgnoreBlock { get; set; }
    public VmEither<Blueprint, Character> Owner { get; set; }
    public List<GraphLink>? InputLinks { get; set; }
    public List<GraphLink>? OutputLinks { get; set; }
    public bool Initial { get; set; }
    public string Name { get; set; }
    public Talking Parent { get; set; }

    private record RawSpeechData(string Id, List<string> ReplyIds, string TextId, string AuthorGuid, bool OnlyOnce,
        bool IsTrade, List<string> EntryPoints, bool IgnoreBlock, string Owner, List<string>? InputLinks, 
        List<string>? OutputLinks, bool Initial, string Name, string ParentId) : RawData(Id);

    public override XElement ToXml(WriterSettings settings) {
        var element = CreateBaseElement(Id);
        if (Replies.Any())
            element.Add(CreateListElement("Replyes", Replies.Select(r => r.Id)));
        element.Add(
            new XElement("Text", Text.Id),
            new XElement("AuthorGuid", AuthorGuid.Id),
            CreateBoolElement("OnlyOnce", OnlyOnce),
            CreateBoolElement("IsTrade", IsTrade)
        );
        if (EntryPoints.Any())
            element.Add(CreateListElement("EntryPoints", EntryPoints.Select(e => e.Id)));
        element.Add(
            CreateBoolElement("IgnoreBlock", IgnoreBlock),
            new XElement("Owner", Owner.Id)
        );
        if (InputLinks?.Any() == true)
            element.Add(CreateListElement("InputLinks", InputLinks.Select(i => i.Id)));
        if (OutputLinks?.Any() == true)
            element.Add(CreateListElement("OutputLinks", OutputLinks.Select(o=> o.Id)));
        element.Add(
            CreateBoolElement("Initial", Initial),
            new XElement("Name", Name),
            new XElement("Parent", Parent.Id)
        );
        return element;
    }

    protected override RawData CreateRawData(XElement element) {
        return new RawSpeechData(
            element.Attribute("id")?.Value ?? throw new ArgumentException("Id missing"),
            ParseListElement(element, "Replyes"),
            GetRequiredElement(element, "Text").Value,
            GetRequiredElement(element, "AuthorGuid").Value,
            ParseBool(GetRequiredElement(element, "OnlyOnce")),
            ParseBool(GetRequiredElement(element, "IsTrade")),
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
        if (rawData is not RawSpeechData data)
            throw new ArgumentException($"Expected RawSpeechData but got {rawData.GetType()}");

        Replies = data.ReplyIds.Select(vm.GetElement<Reply>).ToList();
        Text = vm.GetElement<GameString>(data.TextId);
        AuthorGuid = vm.GetElement<Blueprint, Character>(data.AuthorGuid);
        OnlyOnce = data.OnlyOnce;
        IsTrade = data.IsTrade;
        EntryPoints = data.EntryPoints.Select(vm.GetElement<EntryPoint>).ToList();
        IgnoreBlock = data.IgnoreBlock;
        Owner = vm.GetElement<Blueprint, Character>(data.Owner);
        InputLinks = data.InputLinks?.Select(vm.GetElement<GraphLink>).ToList();
        OutputLinks = data.OutputLinks?.Select(vm.GetElement<GraphLink>).ToList();
        Initial = data.Initial;
        Name = data.Name;
        Parent = vm.GetElement<Talking>(data.ParentId);
    }
    
    protected override VmElement New(VirtualMachine vm, string id, VmElement parent) => throw new NotImplementedException();
    
    public override bool IsOrphaned() => Parent.States.All(r => r.Element != this);
}