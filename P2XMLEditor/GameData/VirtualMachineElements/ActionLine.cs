using System.Collections.Generic;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.GameData.VirtualMachineElements.Enums;
using P2XMLEditor.GameData.VirtualMachineElements.Interfaces;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes;
using P2XMLEditor.Helper;
using P2XMLEditor.Parsing.RawData;

#pragma warning disable CS8618

namespace P2XMLEditor.GameData.VirtualMachineElements;

public class ActionLine(ulong id) : VmElement(id), IFiller<RawActionLineData>, INamedElement {
	public List<VmEither<Action, ActionLine>>? Actions { get; set; }
	public ActionLineType ActionLineType { get; set; }
	public ActionLoopInfo? LoopInfo { get; set; }
	public string Name { get; set; }
	public VmEither<State, Graph, Branch, Talking, Speech> LocalContext { get; set; }
	public int OrderIndex { get; set; }
	public void FillFromRawData(RawActionLineData data, VirtualMachine vm) {
		Actions = [];
		if (data.ActionIds != null)
			foreach (var actionId in data.ActionIds)
				Actions.Add(vm.GetElement<Action,ActionLine>(actionId));
		ActionLineType = data.ActionLineType;
		if (!string.IsNullOrEmpty(data.LoopInfoName) || !string.IsNullOrEmpty(data.LoopInfoStart) || !string.IsNullOrEmpty(data.LoopInfoEnd) || data.LoopInfoRandom.HasValue) {
			LoopInfo = new ActionLoopInfo(
				ParameterSource.Create(data.LoopInfoName ?? "", vm),
				ParameterSource.Create(data.LoopInfoStart ?? "", vm, null, VmTypeInfo.Int32),
				ParameterSource.Create(data.LoopInfoEnd ?? "", vm, null, VmTypeInfo.Int32),
				data.LoopInfoRandom
			);
		} else {
			LoopInfo = null;
		}
		Name = data.Name;
		LocalContext = vm.GetElement<State, Graph, Branch, Talking, Speech>(data.LocalContextId);
		OrderIndex = data.OrderIndex;
	}
	
	public override void OnDestroy(VirtualMachine vm) {
		foreach(var action in Actions ?? [])
			vm.RemoveElement(action.Element);
	}
}
