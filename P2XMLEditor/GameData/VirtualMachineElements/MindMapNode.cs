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

public class MindMapNode(ulong id) : VmElement(id), IFiller<RawMindMapNodeData>, IVmCreator<MindMapNode> {
	public string Name { get; set; }
	public MindMap Parent { get; set; }
	public LogicMapNodeType LogicMapNodeType { get; set; }
	public List<MindMapNodeContent> Content { get; set; }
	public List<MindMapLink> InputLinks { get; set; }
	public List<MindMapLink> OutputLinks { get; set; }
	
	public float GameScreenPosX { get; set; }
	public float GameScreenPosY { get; set; }

	public float? Radius { get; set; } // Demo-only
	public GameString? NodeNameText { get; set; } // Demo-only
	public GameString? NodeDescriptionText { get; set; } // Demo-only
	public (int X, int Y)? GraphPosition { get; set; } // Demo-only
	public bool? Initial { get; set; } // Demo-only

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
		Content = [];
		if (data.ContentIds != null) {
			foreach (var contentId in data.ContentIds)
				Content.Add(vm.GetElement<MindMapNodeContent>(contentId));
		}
		InputLinks = [];
		if (data.InputLinkIds != null) {
			foreach (var inputLinkId in data.InputLinkIds)
				InputLinks.Add(vm.GetElement<MindMapLink>(inputLinkId));
		}
		OutputLinks = [];
		if (data.OutputLinkIds != null) {
			foreach (var outputLinkId in data.OutputLinkIds)
				OutputLinks.Add(vm.GetElement<MindMapLink>(outputLinkId));
		}
		GameScreenPosX = data.GameScreenPosX;
		GameScreenPosY = data.GameScreenPosY;

		Radius = data.Radius;
		NodeNameText = data.NodeNameTextId.HasValue ? vm.GetElement<GameString>(data.NodeNameTextId.Value) : null;
		NodeDescriptionText = data.NodeDescriptionTextId.HasValue ? vm.GetElement<GameString>(data.NodeDescriptionTextId.Value) : null;
		GraphPosition = data.GraphPosition;
		Initial = data.Initial;
	}

	public override void OnDestroy(VirtualMachine vm) {
		Parent.Nodes.Remove(this);
		foreach (var link in InputLinks.Concat(OutputLinks))
			vm.RemoveElement(link);
		foreach (var content in Content)
			vm.RemoveElement(content);
	}
}
