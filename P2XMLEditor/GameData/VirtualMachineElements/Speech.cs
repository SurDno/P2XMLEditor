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

public class Speech(ulong id) : VmElement(id), IFiller<RawSpeechData> {
    protected override HashSet<string> KnownElements { get; } = [
        "Replyes", "Text", "AuthorGuid", "OnlyOnce", "IsTrade", "EntryPoints",
        "IgnoreBlock", "Owner", "InputLinks", "OutputLinks", "Initial", "Name", "Parent"
    ];
    public List<Reply> Replies { get; set; }
    public GameString Text { get; set; }
    public VmEither<Blueprint, Character> AuthorGuid { get; set; }
    public bool? OnlyOnce { get; set; }
    public bool? IsTrade { get; set; }
    public List<EntryPoint> EntryPoints { get; set; }
    public bool? IgnoreBlock { get; set; }
    public VmEither<Blueprint, Character> Owner { get; set; }
    public List<GraphLink>? InputLinks { get; set; }
    public List<GraphLink>? OutputLinks { get; set; }
    public bool? Initial { get; set; }
    public string Name { get; set; }
    public Talking Parent { get; set; }

    public void FillFromRawData(RawSpeechData data, VirtualMachine vm) {
        Replies = new();
        foreach (var replyId in data.ReplyIds) 
            Replies.Add(vm.GetElement<Reply>(replyId));
        Text = vm.GetElement<GameString>(data.TextId);
        AuthorGuid = vm.GetElement<Blueprint, Character>(data.AuthorGuidId);
        OnlyOnce = data.OnlyOnce;
        IsTrade = data.IsTrade;
        EntryPoints = new();
        foreach (var entryPointId in data.EntryPointIds) 
            EntryPoints.Add(vm.GetElement<EntryPoint>(entryPointId));
        IgnoreBlock = data.IgnoreBlock;
        Owner = vm.GetElement<Blueprint, Character>(data.OwnerId);
        InputLinks = new();
        if (data.InputLinkIds != null) {
            foreach (var inputLinkId in data.InputLinkIds)
                InputLinks.Add(vm.GetElement<GraphLink>(inputLinkId));
        }
        OutputLinks = new();
        if (data.OutputLinkIds != null) {
            foreach (var outputLinkId in data.OutputLinkIds)
                OutputLinks.Add(vm.GetElement<GraphLink>(outputLinkId));
        }
        Initial = data.Initial;
        Name = data.Name;
        Parent = vm.GetElement<Talking>(data.ParentId);
    }
    
    public override XElement ToXml(WriterSettings settings) {
        var element = CreateBaseElement(Id);
        if (Replies.Any())
            element.Add(CreateListElement("Replyes", Replies.Select(r => r.Id.ToString())));
        element.Add(
            new XElement("Text", Text.Id),
            new XElement("AuthorGuid", AuthorGuid.Id)
        );
        if (OnlyOnce != null)
            element.Add(CreateBoolElement("OnlyOnce", (bool)OnlyOnce));
        if (IsTrade != null)
            element.Add(CreateBoolElement("IsTrade", (bool)IsTrade));
        if (EntryPoints.Any())
            element.Add(CreateListElement("EntryPoints", EntryPoints.Select(e => e.Id.ToString())) );
        if (IgnoreBlock != null)
            element.Add(CreateBoolElement("IgnoreBlock", (bool)IgnoreBlock));
        element.Add(new XElement("Owner", Owner.Id));
        if (InputLinks?.Any() == true)
            element.Add(CreateListElement("InputLinks", InputLinks.Select(i => i.Id.ToString())));
        if (OutputLinks?.Any() == true)
            element.Add(CreateListElement("OutputLinks", OutputLinks.Select(o => o.Id.ToString())));
        if (Initial != null)
            element.Add(CreateBoolElement("Initial", (bool)Initial));
        element.Add(
            new XElement("Name", Name),
            new XElement("Parent", Parent.Id)
        );
        return element;
    }
    
    public override bool IsOrphaned() => Parent.States.All(r => r.Element != this);
}