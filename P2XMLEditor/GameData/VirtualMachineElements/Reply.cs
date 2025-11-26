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

public class Reply(ulong id) : VmElement(id), IFiller<RawReplyData> {
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
    
    public void FillFromRawData(RawReplyData data, VirtualMachine vm) {
        Name = data.Name;
        Text = vm.GetElement<GameString>(data.TextId);
        OnlyOnce = data.OnlyOnce;
        OnlyOneReply = data.OnlyOneReply;
        Default = data.Default;
        OrderIndex = data.OrderIndex;
        Parent = vm.GetElement<Speech>(data.ParentId);
        EnableCondition = data.EnableConditionId != null ? vm.GetElement<Condition>(data.EnableConditionId.Value) : null;
        ActionLine = data.ActionLineId != null ? vm.GetElement<ActionLine>(data.ActionLineId.Value) : null;
    }
    
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
    
    public override bool IsOrphaned() => Parent.Replies.All(r => r != this);
}