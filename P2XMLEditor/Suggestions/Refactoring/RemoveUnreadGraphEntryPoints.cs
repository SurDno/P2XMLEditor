using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.Logging;

namespace P2XMLEditor.Suggestions.Refactoring;

/// <summary>
/// Removes a graph's entry point where nothing can arrive at it — which means a graph with no
/// input links at all, and not merely one whose links carry no event.
///
/// The first version of this pass got that wrong and had to be corrected, so the reasoning is
/// worth setting out in full.
///
/// A link into a graph does not use the graph's own entry points: ProcessMoveToState routes
/// anything of category GRAPH to MoveIntoSubGraph, which applies the index to the subgraph's
/// initial state instead. That much is true, and it is why the shipped data can carry 866 graphs
/// with no entry point at all in PathologicSandbox. Nothing is lost either — all 7989 graph entry
/// points across the two corpora have an empty action line or none.
///
/// But <c>VMState.GetLocalContextVariables</c> also walks a node's input links, and its bounds
/// check sits outside the branch that uses the result:
///
/// <code>
/// int destEntryPoint = inputLinks[i].DestEntryPoint;
/// if (destEntryPoint &lt; 0 || destEntryPoint >= entryPoints.Count)
///     Logger.AddError("Wrong entry point index");
/// else { ... event return messages ... }
/// </code>
///
/// So the error is logged for <em>every</em> input link whose index is out of range, whether or
/// not that link has an event, every time the graph's local context is resolved — which happens
/// while conditions and expressions on it are updated. The earlier guard only skipped graphs whose
/// links carried an event, because that is what the else branch consumes; it stripped entry points
/// from 3757 graphs in the Sandbox that do have links, and each of those graphs then logs one line
/// per link, 4031 in total per pass.
///
/// The data states the real rule plainly, and this is the check that should have been made first:
/// of the 866 graphs in PathologicSandbox and 5 in MarbleNest that ship without an entry point,
/// every single one has zero input links. Nothing links into a graph that has none.
///
/// So the condition is no input links, no incoming substitution, not the initial node, and an
/// action line with nothing in it. That leaves 1210 graphs in PathologicSandbox and 290 in
/// MarbleNest. To repair data the earlier version damaged, see
/// <see cref="RestoreGraphEntryPoints"/>.
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
		var keptLinked = 0;
		var keptSubstituted = 0;

		foreach (var graph in Vm.GetElementsByType<Graph>().ToList()) {
			foreach (var point in (graph.EntryPoints ?? []).ToList()) {
				// Nothing here judges whether the actions are worth keeping — an entry point that
				// runs something is one whose removal would change what the game does, and that is
				// not a decision for a bulk pass.
				if (point.ActionLine is { Actions.Count: > 0 }) { keptWithActions++; continue; }
				if (graph.Initial) { keptInitial++; continue; }

				// Any link at all, evented or not: GetLocalContextVariables checks the index of
				// every one of them and logs an error when the list is empty.
				if (inbound.TryGetValue(graph.Id, out var links) && links.Count > 0) {
					keptLinked++;
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
			+ $"{keptInitial} on an initial graph, {keptLinked} with input links that would then log "
			+ $"\"Wrong entry point index\", {keptSubstituted} substituted by another graph.");
	}
}
