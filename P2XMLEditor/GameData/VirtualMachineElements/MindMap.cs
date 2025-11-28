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
using static P2XMLEditor.Helper.XmlParsingHelper;

#pragma warning disable CS8618

namespace P2XMLEditor.GameData.VirtualMachineElements;

public class MindMap(ulong id) : VmElement(id), IFiller<RawMindMapData>, IVmCreator<MindMap> {
    protected override HashSet<string> KnownElements { get; } =
        ["Name", "LogicMapType", "Title", "Parent", "Nodes", "Links"];
    public string Name { get; set; }
    public LogicMapType LogicMapType { get; set; }
    public GameString Title { get; set; }
    public GameRoot Parent { get; set; }
    public List<MindMapNode> Nodes { get; set; } = [];
    public List<MindMapLink> Links { get; set; } = [];
    
    public override XElement ToXml(WriterSettings settings) {
        var element = CreateBaseElement(Id);
        if (Nodes.Count != 0)
            element.Add(CreateListElement("Nodes", Nodes.Select(n => n.Id.ToString())));
        if (Links.Count != 0)
            element.Add(CreateListElement("Links", Links.Select(l => l.Id.ToString())));
        element.Add(
            new XElement("LogicMapType", LogicMapType.Serialize()),
            new XElement("Title", Title.Id),
            new XElement("Name", Name),
            new XElement("Parent", Parent.Id)
        );
        return element;
    }
    
    public void FillFromRawData(RawMindMapData data, VirtualMachine vm) {
        Name = data.Name;
        LogicMapType = data.LogicMapType;
        Title = vm.GetElement<GameString>(data.TitleId);
        Parent = vm.GetElement<GameRoot>(data.ParentId);
        Nodes = new();
        foreach (var nodeId in data.NodeIds)
            Nodes.Add(vm.GetElement<MindMapNode>(nodeId));
        Links = new();
        if (data.LinkIds == null) return;
        foreach (var nodeId in data.LinkIds)
            Links.Add(vm.GetElement<MindMapLink>(nodeId));
    }
    
    public static MindMap New(VirtualMachine vm, ulong id, VmElement parent) {
        var node = new MindMap(id) {
            Name = "New Mind Map",
            LogicMapType = LogicMapType.Global,
            Parent = parent as GameRoot ?? throw new ArgumentException("MindMap parent must be GameRoot")
        };
        node.Title = CreateDefault<GameString>(vm, node.Parent);
        node.Parent.LogicMaps.Add(node);
        return node;
    }

    public void OnDestroy(VirtualMachine vm) {
        foreach (var node in Nodes)
            vm.RemoveElement(node);
        foreach (var link in Links)
            vm.RemoveElement(link);
        vm.RemoveElement(Title);
        Parent.LogicMaps.Remove(this);
    }
}