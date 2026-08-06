using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using P2XMLEditor.Core;
using P2XMLEditor.Data;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.GameData.VirtualMachineElements.Enums;
using P2XMLEditor.GameData.VirtualMachineElements.Interfaces;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes;
using P2XMLEditor.Helper;
using P2XMLEditor.Parsing.RawData;
using static P2XMLEditor.Helper.XmlParsingHelper;

#pragma warning disable CS8618

namespace P2XMLEditor.GameData.VirtualMachineElements;

public class Branch(ulong id) : VmElement(id), IGraphElement, IFiller<RawBranchData> {
	public List<VmEither<Condition, PartCondition>> BranchConditions { get; set; }
	public BranchType BranchType { get; set; }
	public List<BranchVariantInfo>? BranchVariantInfo { get; set; }
	public VmEither<Graph, Talking> Parent { get; set; }
	
	public List<EntryPoint> EntryPoints { get; set; }
	public List<GraphLink>? InputLinks { get; set; }
	public List<GraphLink>? OutputLinks { get; set; }
	public ParameterHolder Owner { get; set; }
	public string Name { get; set; }
	public bool IgnoreBlock { get; set; }
	public bool Initial { get; set; }
	
	public override bool IsOrphaned() {
		return Parent.Element switch {
			Graph g => g.States.All(r => r.Element != this),
			Talking t => t.States.All(r => r.Element != this),
			_ => true
		};
	}
	
	/// <summary>
	/// A case branch with no conditions. That is not a degenerate state: a branch always has
	/// one more exit than it has conditions — the one taken when none matched — so a fresh
	/// branch already has somewhere for a link to leave from.
	/// </summary>
	public static Branch New(VirtualMachine vm, ulong id, VmElement parent) {
		var owner = parent switch {
			Graph g => g.Owner,
			Talking t => t.Owner.Element as ParameterHolder,
			_ => null
		};
		var branch = new Branch(id) {
			Name = "New branch",
			BranchType = BranchType.Case,
			BranchConditions = [],
			Parent = new(parent),
			Owner = owner!,
			EntryPoints = [],
			InputLinks = [],
			OutputLinks = [],
			IgnoreBlock = false,
			Initial = false
		};
		branch.EntryPoints.Add(CreateDefault<EntryPoint>(vm, branch));
		return branch;
	}

	public void FillFromRawData(RawBranchData data, VirtualMachine vm) {
		BranchConditions = [];
		if (data.BranchConditionIds != null) {
			foreach (var branchConditionId in data.BranchConditionIds)
				BranchConditions.Add(vm.GetElement<Condition, PartCondition>(branchConditionId));
		}
		BranchType = data.BranchType;
		Parent = vm.GetElement<Graph, Talking>(data.ParentId);
		BranchVariantInfo = data.BranchVariantInfo?
			.Select(t => InternalTypes.BranchVariantInfo.Read(t.Item1, t.Item2, vm, this))
			.ToList();
		EntryPoints = [];
		foreach (var entryPointId in data.EntryPointIds)
			EntryPoints.Add(vm.GetElement<EntryPoint>(entryPointId));
		IgnoreBlock = data.IgnoreBlock;
		Owner = vm.GetElement<ParameterHolder>(data.OwnerId);
		InputLinks = [];
		if (data.InputLinkIds != null) {
			foreach (var inputLinkId in data.InputLinkIds)
				InputLinks.Add(vm.GetElement<GraphLink>(inputLinkId));
		}
		OutputLinks = [];
		if (data.OutputLinkIds != null) {
			foreach (var outputLinkId in data.OutputLinkIds)
				OutputLinks.Add(vm.GetElement<GraphLink>(outputLinkId));
		}
		Initial = data.Initial;
		Name = data.Name;
	}
	
	public override void OnDestroy(VirtualMachine vm) {
		foreach (var link in InputLinks?.ToList() ?? []) 
			vm.RemoveElement(link);
		foreach (var link in OutputLinks?.ToList() ?? []) 
			vm.RemoveElement(link);
		foreach (var entryPoint in EntryPoints?.ToList() ?? []) 
			vm.RemoveElement(entryPoint);
		foreach (var cond in BranchConditions?.ToList() ?? []) 
			vm.RemoveElement(cond.Element);
			
		if (Parent.Element is Graph parentGraph) {
			parentGraph.States?.RemoveAll(s => s.Element == this);
		} else if (Parent.Element is Talking parentTalking) {
			parentTalking.States?.RemoveAll(s => s.Element == this);
		}
	}
}