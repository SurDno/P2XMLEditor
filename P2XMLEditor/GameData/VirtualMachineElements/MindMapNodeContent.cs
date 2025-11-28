using System;
using System.Collections.Generic;
using System.Xml.Linq;
using P2XMLEditor.Core;
using P2XMLEditor.Data;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.GameData.VirtualMachineElements.Enums;
using P2XMLEditor.GameData.VirtualMachineElements.Interfaces;
using P2XMLEditor.Helper;
using P2XMLEditor.Parsing.RawData;
using static P2XMLEditor.Helper.XmlParsingHelper;

#pragma warning disable CS8618

namespace P2XMLEditor.GameData.VirtualMachineElements;

public class MindMapNodeContent(ulong id) : VmElement(id), IFiller<RawMindMapNodeContentData>, IVmCreator<MindMapNodeContent> {
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
    
    public void FillFromRawData(RawMindMapNodeContentData data, VirtualMachine vm) {
        Parent = vm.GetElement<MindMapNode>(data.ParentId);
        ContentType = data.ContentType;
        Number = data.Number;
        ContentDescriptionText = vm.GetElement<GameString>(data.ContentDescriptionTextId);
        ContentPicture = data.ContentPictureId.HasValue ? 
            vm.GetElement<Sample>(data.ContentPictureId.Value) : null;
        ContentCondition = vm.GetElement<Condition>(data.ContentConditionId);
        Name = data.Name;
    }
    
    public static MindMapNodeContent New(VirtualMachine vm, ulong id, VmElement parent) => new MindMapNodeContent(id) {
        Name = "Initial Content",
        ContentType = NodeContentType.Info,
        Parent = parent as MindMapNode ?? throw new ArgumentException("Parent must be MindMapNode"),
        ContentPicture = vm.First<Sample>(s => s.SampleType is SampleType.MindMapPicture),
        ContentDescriptionText = CreateDefault<GameString>(vm, (parent as MindMapNode)!.Parent),
        ContentCondition = CreateDefault<Condition>(vm, null!)
    };

    public void OnDestroy(VirtualMachine vm) {
        vm.RemoveElement(ContentDescriptionText);
        vm.RemoveElement(ContentCondition);
        Parent.Content.Remove(this);
    }
}