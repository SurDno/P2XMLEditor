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

public class Talking(ulong id) : VmElement(id), IFiller<RawTalkingData> {
	public List<VmEither<Branch, Speech, State>> States { get; set; }
	public List<GraphLink> EventLinks { get; set; }
	public List<EntryPoint> EntryPoints { get; set; }
	public List<GraphLink> InputLinks { get; set; }
	public bool? IgnoreBlock { get; set; }
	public VmEither<Blueprint, Character> Owner { get; set; }
	public bool? Initial { get; set; }
	public string Name { get; set; }
	public Graph Parent { get; set; }
	
	public void FillFromRawData(RawTalkingData data, VirtualMachine vm) {
		States = data.StateIds.Select(vm.GetElement<Branch, Speech, State>).ToList();
		EventLinks = [];
		foreach (var eventLinkId in data.EventLinkIds) 
			EventLinks.Add(vm.GetElement<GraphLink>(eventLinkId));
		EntryPoints = [];
		foreach (var entryPointId in data.EntryPointIds) 
			EntryPoints.Add(vm.GetElement<EntryPoint>(entryPointId));
		IgnoreBlock = data.IgnoreBlock;
		Owner = vm.GetElement<Blueprint, Character>(data.OwnerId);
		InputLinks = [];
		if (data.InputLinkIds != null) {
			foreach (var inputLinkId in data.InputLinkIds) 
				InputLinks.Add(vm.GetElement<GraphLink>(inputLinkId));
		}
		Initial = data.Initial;
		Name = data.Name;
		Parent = vm.GetElement<Graph>(data.ParentId);
	}
	
	public override bool IsOrphaned() => Parent.States.All(r => r.Element != this);

	public string ParamId => id.ToString();
}
