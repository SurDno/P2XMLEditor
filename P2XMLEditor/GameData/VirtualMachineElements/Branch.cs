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
	public bool? IgnoreBlock { get; set; }
	public bool? Initial { get; set; }
	
	public override bool IsOrphaned() {
		return Parent.Element switch {
			Graph g => g.States.All(r => r.Element != this),
			Talking t => t.States.All(r => r.Element != this),
			_ => true
		};
	}
	
	public void FillFromRawData(RawBranchData data, VirtualMachine vm) {
		BranchConditions = [];
		if (data.BranchConditionIds != null) {
			foreach (var branchConditionId in data.BranchConditionIds)
				BranchConditions.Add(vm.GetElement<Condition, PartCondition>(branchConditionId));
		}
		BranchType = data.BranchType;
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
		Parent = vm.GetElement<Graph, Talking>(data.ParentId);
	}
	
	public override void OnDestroy(VirtualMachine vm) {
		foreach (var link in InputLinks ?? []) 
			vm.RemoveElement(link);
		foreach (var entryPoint in EntryPoints) 
			vm.RemoveElement(entryPoint);
	}
}