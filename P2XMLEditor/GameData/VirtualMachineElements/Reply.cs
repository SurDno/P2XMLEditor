using System.Xml.Linq;
using P2XMLEditor.Core;
using P2XMLEditor.Data;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.Helper;
using static P2XMLEditor.Helper.XmlParsingHelper;

#pragma warning disable CS8618

namespace P2XMLEditor.GameData.VirtualMachineElements;

public class Reply(string id) : VmElement(id) {
    protected override HashSet<string> KnownElements { get; } = [
        "Name", "Text", "OnlyOnce", "OnlyOneReply", "Default", "OrderIndex", "Parent",
        "EnableCondition", "ActionLine"
    ];
    public string Name { get; set; }
    public GameString Text { get; set; }
    public bool? OnlyOnce { get; set; }
    public bool? OnlyOneReply { get; set; }
    public bool? Default { get; set; }
    public Condition? EnableCondition { get; set; }
    public ActionLine? ActionLine { get; set; }
    public int OrderIndex { get; set; }
    public Speech Parent { get; set; }

    private record RawReplyData(string Id, string Name, string TextId, bool? OnlyOnce, bool? OnlyOneReply, bool? Default,
        string? EnableConditionId, string? ActionLineId, int OrderIndex, string ParentId) : RawData(Id);
    
    public override XElement ToXml(WriterSettings settings) {
        var element = CreateBaseElement(Id);
        element.Add(
            new XElement("Name", Name),
            new XElement("Text", Text.Id)
        );
        if (OnlyOnce != null)
            element.Add(CreateBoolElement("OnlyOnce", (bool)OnlyOnce));
        if (OnlyOneReply != null)
            element.Add(CreateBoolElement("OnlyOneReply", (bool)OnlyOneReply));
        if (Default != null)
            element.Add(CreateBoolElement("Default", (bool)Default));
        if (EnableCondition != null)
            element.Add(new XElement("EnableCondition", EnableCondition.Id));
        if (ActionLine != null)
            element.Add(new XElement("ActionLine", ActionLine.Id));
        element.Add(
            new XElement("OrderIndex", OrderIndex),
            new XElement("Parent", Parent.Id)
        );
        return element;
    }

    protected override RawData CreateRawData(XElement element) {
        return new RawReplyData(
            element.Attribute("id")?.Value ?? throw new ArgumentException("Id missing"),
            GetRequiredElement(element, "Name").Value,
            GetRequiredElement(element, "Text").Value,
            element.Element("OnlyOnce")?.Let(ParseBool),
            element.Element("OnlyOneReply")?.Let(ParseBool),
            element.Element("Default")?.Let(ParseBool),
            element.Element("EnableCondition")?.Value,
            element.Element("ActionLine")?.Value, 
            GetRequiredElement(element, "OrderIndex").ParseInt(),
            GetRequiredElement(element, "Parent").Value
        );
    }
    
    public override void FillFromRawData(RawData rawData, VirtualMachine vm) {
        if (rawData is not RawReplyData data)
            throw new ArgumentException($"Expected RawReplyData but got {rawData.GetType()}");

        Name = data.Name;
        Text = vm.GetElement<GameString>(data.TextId);
        OnlyOnce = data.OnlyOnce;
        OnlyOneReply = data.OnlyOneReply;
        Default = data.Default;
        OrderIndex = data.OrderIndex;
        Parent = vm.GetElement<Speech>(data.ParentId);
        EnableCondition = data.EnableConditionId != null ? vm.GetElement<Condition>(data.EnableConditionId) : null;
        ActionLine = data.ActionLineId != null ? vm.GetElement<ActionLine>(data.ActionLineId) : null;
    }
    
    protected override VmElement New(VirtualMachine vm, string id, VmElement parent) => throw new NotImplementedException();
    
    public override bool IsOrphaned() => Parent.Replies.All(r => r != this);
}