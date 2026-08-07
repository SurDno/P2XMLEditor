using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.Logging;

namespace P2XMLEditor.Suggestions.Refactoring;

/// <summary>
/// Removes the entry point of a graph, where the engine never reads it.
///
/// An entry point is not dead weight just because it is empty, and this is worth stating plainly
/// because the opposite is the obvious guess. <c>FSMGraphManager.MoveIntoState</c> refuses the
/// move outright when <c>destEntryPoint >= newState.EntryPoints.Count</c> — it logs "Invalid
/// entry point index" and the node is never entered. Every node in the shipped data has exactly
/// one entry point and all 38 286 links name index 0, so taking that one away from a state, a
/// branch, a speech or a talking would stop it being entered at all. Emptiness has nothing to do
/// with it.
///
/// A graph is the exception, because a link into one never goes through that path.
/// <c>ProcessMoveToState</c> routes anything of category GRAPH to <c>MoveIntoSubGraph</c>, which
/// pushes the state, fills the input params, and then calls
/// <c>MoveIntoState(subGraph.InitState, destEntryPoint)</c> — the index is applied to the
/// subgraph's initial state, never to the graph. The graph's own list would only be read if the
/// graph were itself an initial node, and no graph in either corpus is one (0 of 8019, 0 of 841).
/// Nor is anything lost: all 7989 graph entry points across the two corpora have an empty action
/// line or none at all.
///
/// Two things do still read the list, which is what the guards below are for:
///
/// * <c>VMState.GetLocalContextVariables</c> adds an input link's event return messages to the
///   local scope only when that link's DestEntryPoint indexes this node's own entry points.
///   Emptying the list would drop those messages. Graphs whose input links carry an event are
///   therefore left alone — and tellingly, of the 871 graphs that already ship with no entry
///   point, not one has an evented input link.
/// * <c>FiniteStateMachine.EntryPoints</c> returns the substitute's list when a graph has a
///   SubstituteGraph, so a graph that another graph substitutes is left alone as well; its list
///   is read on somebody else's behalf.
///
/// What is left qualifies: 4967 graphs in PathologicSandbox and 585 in MarbleNest.
/// </summary>
[Refactoring("Refactor/Graphs/Remove unread graph entry points"), SuppressMessage("ReSharper", "UnusedType.Global")]
public class RemoveUnreadGraphEntryPoints(VirtualMachine vm) : Suggestion(vm) {
	public override void Execute() {
		var inbound = new Dictionary<ulong, List<GraphLink>>();
		foreach (var link in Vm.GetElementsByType<GraphLink>()) {
			if (link.Destination?.Element is not { } destination) continue;
			if (!inbound.TryGetValue(destination.Id, out var list)) inbound[destination.Id] = list = [];
			list.Add(link);
		}

		var substituted = Vm.GetElementsByType<Graph>()
			.Select(g => g.SubstituteGraph?.Element.Id)
			.Where(id => id != null)
			.Select(id => id!.Value)
			.ToHashSet();

		var removed = 0;
		var keptWithActions = 0;
		var keptInitial = 0;
		var keptEvented = 0;
		var keptSubstituted = 0;

		foreach (var graph in Vm.GetElementsByType<Graph>().ToList()) {
			foreach (var point in (graph.EntryPoints ?? []).ToList()) {
				// Nothing here judges whether the actions are worth keeping — an entry point that
				// runs something is one whose removal would change what the game does, and that is
				// not a decision for a bulk pass.
				if (point.ActionLine is { Actions.Count: > 0 }) { keptWithActions++; continue; }
				if (graph.Initial) { keptInitial++; continue; }
				if (inbound.TryGetValue(graph.Id, out var links) && links.Any(l => l.Event != null)) {
					keptEvented++;
					continue;
				}
				if (substituted.Contains(graph.Id)) { keptSubstituted++; continue; }

				// OnDestroy takes the empty action line with it and unhooks the point from the
				// graph's own list, so this is the whole edit.
				Vm.RemoveElement(point);
				removed++;
			}
		}

		Logger.Log(LogLevel.Info,
			$"Removed {removed} unread graph entry point(s). Kept: {keptWithActions} that run actions, "
			+ $"{keptInitial} on an initial graph, {keptEvented} whose input links carry an event, "
			+ $"{keptSubstituted} substituted by another graph.");
	}
}
