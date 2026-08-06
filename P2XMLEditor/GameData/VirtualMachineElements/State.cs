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

public class State(ulong id) : VmElement(id), IFiller<RawStateData>, IGraphElement {
	public Graph Parent { get; set; }
	
	public List<EntryPoint> EntryPoints { get; set; }
	public List<GraphLink>? InputLinks { get; set; }
	public List<GraphLink>? OutputLinks { get; set; }
	public ParameterHolder Owner { get; set; }
	public string Name { get; set; }
	public bool IgnoreBlock { get; set; }
	public bool Initial { get; set; }
	
	public void FillFromRawData(RawStateData data, VirtualMachine vm) {
		EntryPoints = [];
		foreach (var entrypoint in data.EntryPointIds)
			EntryPoints.Add(vm.GetElement<EntryPoint>(entrypoint));
		IgnoreBlock = data.IgnoreBlock;
		Owner = vm.GetElement<ParameterHolder>(data.OwnerId);
		InputLinks = [];
		if (data.InputLinkIds != null)
		  foreach (var entrypoint in data.InputLinkIds)
			 InputLinks.Add(vm.GetElement<GraphLink>(entrypoint));
		OutputLinks = [];
		if (data.OutputLinkIds != null)
			foreach (var entrypoint in data.OutputLinkIds)
			  OutputLinks.Add(vm.GetElement<GraphLink>(entrypoint));
		Initial = data.Initial;
		Name = data.Name;
		Parent = vm.GetElement<Graph>(data.ParentId);
	}
	
	/// <summary>
	/// A state in <paramref name="parent"/>, with the one entry point a node needs to be
	/// reachable at all — a link has to arrive somewhere, and index 0 is where it arrives.
	/// The caller adds it to the graph's States; nothing here does, so a cancelled creation
	/// leaves no half-attached node behind.
	/// </summary>
	public static State New(VirtualMachine vm, ulong id, VmElement parent) {
		var graph = parent as Graph ?? throw new System.ArgumentException("A state lives in a graph.");
		var state = new State(id) {
			Name = "New state",
			Parent = graph,
			Owner = graph.Owner,
			EntryPoints = [],
			InputLinks = [],
			OutputLinks = [],
			IgnoreBlock = false,
			Initial = false
		};
		state.EntryPoints.Add(CreateDefault<EntryPoint>(vm, state));
		return state;
	}

	public override bool IsOrphaned() => Parent.States.All(r => r.Element != this);
	
	public override void OnDestroy(VirtualMachine vm) {
		foreach (var link in InputLinks?.ToList() ?? []) 
			vm.RemoveElement(link);
		foreach (var link in OutputLinks?.ToList() ?? []) 
			vm.RemoveElement(link);
		foreach (var entryPoint in EntryPoints?.ToList() ?? []) 
			vm.RemoveElement(entryPoint);
		Parent?.States?.RemoveAll(s => s.Element == this);
	}
}
