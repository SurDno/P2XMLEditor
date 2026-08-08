using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.Logging;

namespace P2XMLEditor.Suggestions.Refactoring;

/// <summary>
/// Removes the implicit "_state" parameter from objects that have no event graph.
///
/// The parameter is the object's current FSM state, and an object with no event graph has no
/// FSM: <c>FSMParamsManager</c> exists per DynamicFSM, so nothing ever builds a DynamicParameter
/// for it and nothing can read it. Unlike the value — which the engine discards even on objects
/// that do have a graph, see <see cref="StripStateParameterValues"/> — the parameter itself is
/// worth keeping where there is an FSM, because that is what the reference resolves against.
///
/// It does occur in shipped data, just barely: 8 objects in MarbleNest and none in
/// PathologicSandbox. All 8 are leaves that nothing inherits from, which is the case this checks
/// — a blueprint's parameters are shared with everything derived from it, so a graphless
/// blueprint whose children have graphs is not graphless as far as the parameter is concerned.
/// </summary>
[Refactoring("Refactor/Parameters/Remove state parameters from objects with no graph"),
 SuppressMessage("ReSharper", "UnusedType.Global")]
public class RemoveStateParametersFromGraphlessObjects(VirtualMachine vm) : Suggestion(vm) {
	public override void Execute() {
		var descendants = Descendants();

		var removed = 0;
		var keptInherited = 0;

		foreach (var holder in Vm.GetElementsByType<ParameterHolder>().ToList()) {
			if (holder.CustomParams == null) continue;

			var state = holder.CustomParams
				.Where(pair => StripStateParameterValues.IsImplicitState(pair.Value))
				.ToList();
			if (state.Count == 0) continue;

			if (HasGraph(holder)) continue;
			if (descendants.TryGetValue(holder.Id, out var children) && children.Any(HasGraph)) {
				keptInherited += state.Count;
				continue;
			}

			foreach (var (key, parameter) in state) {
				holder.CustomParams.Remove(key);
				Vm.RemoveElement(parameter);
				removed++;
				Logger.Log(LogLevel.Info, $"Removed {key} from {holder.Name}, which has no event graph");
			}
		}

		Logger.Log(LogLevel.Info,
			$"Removed {removed} state parameter(s) from objects with no event graph"
			+ (keptInherited > 0 ? $"; kept {keptInherited} inherited by an object that has one." : "."));
	}

	private static bool HasGraph(ParameterHolder holder) => holder.EventGraph != null;

	/// <summary>Everything that derives from each object, so an inherited parameter is not judged alone.</summary>
	private Dictionary<ulong, List<ParameterHolder>> Descendants() {
		var direct = new Dictionary<ulong, List<ParameterHolder>>();
		foreach (var holder in Vm.GetElementsByType<ParameterHolder>())
			foreach (var prototype in holder.InheritanceInfo ?? [])
				if (ulong.TryParse(prototype, out var id)) {
					if (!direct.TryGetValue(id, out var list)) direct[id] = list = [];
					list.Add(holder);
				}

		// Flatten, so a graph three generations down still counts.
		var all = new Dictionary<ulong, List<ParameterHolder>>();
		foreach (var (id, _) in direct) {
			var collected = new List<ParameterHolder>();
			var seen = new HashSet<ulong>();
			var pending = new Queue<ulong>();
			pending.Enqueue(id);

			while (pending.Count > 0) {
				var current = pending.Dequeue();
				if (!seen.Add(current) || !direct.TryGetValue(current, out var children)) continue;
				foreach (var child in children) {
					collected.Add(child);
					pending.Enqueue(child.Id);
				}
			}

			all[id] = collected;
		}

		return all;
	}
}
