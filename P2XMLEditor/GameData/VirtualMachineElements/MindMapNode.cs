using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using P2XMLEditor.Core;
using P2XMLEditor.Data;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.GameData.VirtualMachineElements.Enums;
using P2XMLEditor.GameData.VirtualMachineElements.Interfaces;
using P2XMLEditor.Helper;
using P2XMLEditor.Parsing.RawData;
using ZLinq;
using static P2XMLEditor.Helper.XmlParsingHelper;

#pragma warning disable CS8618

namespace P2XMLEditor.GameData.VirtualMachineElements;

public class MindMapNode(ulong id) : VmElement(id), IFiller<RawMindMapNodeData>, IVmCreator<MindMapNode> {
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
    
    public override XElement ToXml(WriterSettings settings) {
        var element = CreateBaseElement(Id);
        element.Add(
            new XElement("LogicMapNodeType", LogicMapNodeType.Serialize())
        );
        if (Content.Count != 0)
            element.Add(CreateListElement("NodeContent", Content.Select(c => c.Id.ToString())));
        element.Add(
            new XElement("GameScreenPosX", FormatPos(GameScreenPosX)),
            new XElement("GameScreenPosY", FormatPos(GameScreenPosY))
        );
        if (InputLinks.Count != 0)
            element.Add(CreateListElement("InputLinks", InputLinks.Select(l => l.Id.ToString())));
        if (OutputLinks.Count != 0)
            element.Add(CreateListElement("OutputLinks", OutputLinks.Select(l => l.Id.ToString())));
        element.Add(
            new XElement("Name", Name),
            new XElement("Parent", Parent.Id)
        );
        return element;
    }
    
    private static string FormatPos(float value) {
        const float posStep = 0.0025f;
        var str = (MathF.Round(value / posStep) * posStep).ToString("0.#####");
        return str.Contains('.') ? str.TrimEnd('0').TrimEnd('.') : str;
    }

    public static MindMapNode New(VirtualMachine vm, ulong id, VmElement parent) {
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
    
    public void FillFromRawData(RawMindMapNodeData data, VirtualMachine vm) {
        Name = data.Name;
        Parent = vm.GetElement<MindMap>(data.ParentId);
        LogicMapNodeType = data.LogicMapNodeType;
        Content = new();
        if (data.ContentIds != null) {
            foreach (var contentId in data.ContentIds)
                Content.Add(vm.GetElement<MindMapNodeContent>(contentId));
        }
        InputLinks = new();
        if (data.InputLinkIds != null) {
            foreach (var inputLinkId in data.InputLinkIds)
                InputLinks.Add(vm.GetElement<MindMapLink>(inputLinkId));
        }
        OutputLinks = new();
        if (data.OutputLinkIds != null) {
            foreach (var outputLinkId in data.OutputLinkIds)
                OutputLinks.Add(vm.GetElement<MindMapLink>(outputLinkId));
        }
        GameScreenPosX = data.GameScreenPosX;
        GameScreenPosY = data.GameScreenPosY;
    }

    public void OnDestroy(VirtualMachine vm) {
        Parent.Nodes.Remove(this);
        foreach (var link in InputLinks.Concat(OutputLinks))
            vm.RemoveElement(link);
        foreach (var content in Content)
            vm.RemoveElement(content);
    }
}