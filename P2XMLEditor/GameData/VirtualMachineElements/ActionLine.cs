using System.Collections.Generic;
using System.Linq;
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
		if (!string.IsNullOrEmpty(data.LoopInfoName) || !string.IsNullOrEmpty(data.LoopInfoStart) || !string.IsNullOrEmpty(data.LoopInfoEnd) || data.LoopInfoRandom) {
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
	
	/// <summary>
	/// An empty line in the local context of <paramref name="parent"/>. A line's context is the
	/// node its actions run in, which is what every reference inside them resolves against, so a
	/// parent that is not a node is walked up until one is.
	/// </summary>
	public static ActionLine New(VirtualMachine vm, ulong id, VmElement parent) => new(id) {
		Name = "New line",
		ActionLineType = ActionLineType.Common,
		Actions = [],
		OrderIndex = 0,
		LocalContext = new(LocalContextOf(parent))
	};

	/// <summary>
	/// The nearest enclosing node an action can name as its context. An entry point, an action
	/// line or an action stands in for the node that owns it.
	/// </summary>
	internal static VmElement LocalContextOf(VmElement element) {
		for (var guard = 0; guard < 16; guard++) {
			switch (element) {
				case State or Graph or Branch or Talking or Speech:
					return element;
				case EntryPoint point:
					element = point.Parent?.Element;
					break;
				case ActionLine line:
					element = line.LocalContext.Element;
					break;
				case Action action:
					element = action.LocalContext.Element;
					break;
				default:
					return element;
			}
		}
		return element;
	}

	public override void OnDestroy(VirtualMachine vm) {
		foreach(var action in Actions?.ToList() ?? [])
			vm.RemoveElement(action.Element);
	}
}
