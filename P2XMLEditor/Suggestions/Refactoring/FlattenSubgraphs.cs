using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.GameData.VirtualMachineElements.Enums;
using P2XMLEditor.GameData.VirtualMachineElements.Helper;
using P2XMLEditor.GameData.VirtualMachineElements.Interfaces;
using P2XMLEditor.Logging;

namespace P2XMLEditor.Suggestions.Refactoring;

/// <summary>
/// Puts a subgraph's nodes directly into its parent and deletes the subgraph, where that provably
/// changes nothing about how the graph runs.
///
/// Most of this pass is the list of cases it refuses, because a subgraph is not a container — it
/// is a node with its own scope and its own way in and out, and four of those things stop being
/// true when its contents are poured into the parent:
///
/// * <b>Input parameters.</b> Links into a subgraph carry arguments for them. Flattened, there is
///   nowhere for those arguments to go.
/// * <b>Substitution.</b> A graph that substitutes another, or is substituted by one, is entered
///   through that relationship rather than through its own contents.
/// * <b>Returning to the previous state.</b> An inner link with no destination and LinkExit 0 goes
///   back to the state the FSM was in before — which, inside a subgraph, is the state before the
///   subgraph node. In the parent it would mean something else entirely. 374 subgraphs in
///   PathologicSandbox contain one.
/// * <b>Messages arriving with the entry.</b> A state inside a subgraph can read the messages of
///   the event that entered it; whether it still can afterwards depends on a walk this pass would
///   have to reproduce exactly to be sure of. Subgraphs whose incoming links carry an event are
///   left alone rather than reasoned about — 309 of them.
///
/// Procedures are skipped too. GRAPH_TYPE_PROCEDURE is entered by SwitchState rather than by
/// pushing a state, and MoveIntoSubGraph branches on it; that is a different execution shape, not
/// a nesting convenience.
///
/// What is left is 407 subgraphs in PathologicSandbox and 49 in MarbleNest.
///
/// The earlier version of this was commented out with "todo: rework", and one reason is worth
/// recording: it rewired the incoming links to the inner state but left them in the subgraph's
/// InputLinks, so removing the subgraph took every one of them with it — Graph.OnDestroy deletes
/// its input and output links. Everything the pass keeps has to be unhooked from the subgraph
/// before the subgraph goes.
/// </summary>
[Refactoring("Refactor/Graphs/Flatten strictly safe subgraphs"), SuppressMessage("ReSharper", "UnusedType.Global")]
public class FlattenSubgraphs(VirtualMachine vm) : Suggestion(vm) {
	public override void Execute() {
		var substituted = Vm.GetElementsByType<Graph>()
			.Select(g => g.SubstituteGraph?.Element.Id)
			.Where(id => id != null)
			.Select(id => id!.Value)
			.ToHashSet();

		var flattened = 0;
		var skipped = new Dictionary<string, int>();

		foreach (var subgraph in Vm.GetElementsByType<Graph>().ToList()) {
			if (subgraph.Parent.Element is not Graph parent) continue;
			if (Refuse(subgraph, parent, substituted) is { } reason) {
				skipped[reason] = skipped.GetValueOrDefault(reason) + 1;
				continue;
			}

			Flatten(subgraph, parent);
			flattened++;
		}

		Logger.Log(LogLevel.Info, $"Flattened {flattened} subgraph(s).");
		foreach (var (reason, count) in skipped.OrderByDescending(p => p.Value))
			Logger.Log(LogLevel.Info, $"   left alone, {count}: {reason}");
	}

	/// <summary>Why this subgraph cannot be poured into its parent, or null when it can.</summary>
	private string? Refuse(Graph subgraph, Graph parent, IReadOnlySet<ulong> substituted) {
		if (subgraph.States is not { Count: > 0 }) return "it has no nodes of its own";
		if (subgraph.GraphType != GraphType.EventGraph) return "it is a procedure, not an event graph";
		if (subgraph.InputParams is { Count: > 0 }) return "it takes input parameters";
		if (subgraph.SubstituteGraph != null || substituted.Contains(subgraph.Id))
			return "it substitutes another graph or is substituted by one";

		if (subgraph.States.Count(s => GraphTopology.IsInitial(s.Element)) != 1)
			return "it does not have exactly one initial node";

		var exits = GraphTopology.LinksFrom(subgraph);
		if (exits.Count > 1) return "more than one link leaves the subgraph node";
		if (exits.Any(l => l.Event != null)) return "the link leaving it waits for an event";

		var entries = GraphTopology.LinksInto(subgraph);
		if (entries.Any(l => l.Event != null)) return "a link into it carries an event, which its nodes may read";

		var inner = subgraph.EventLinks ?? [];
		if (inner.Any(l => GraphTopology.IsTerminator(l) &&
						   GraphTopology.ExitTypeOf(l) == GraphTopology.LinkExit.PreviousState))
			return "a link inside it returns to the previous state, which would then mean another state";
		if (exits.Count == 0 && inner.Any(l => GraphTopology.IsTerminator(l) &&
											   GraphTopology.ExitTypeOf(l) == GraphTopology.LinkExit.OuterGraph))
			return "it leaves itself but the subgraph node has nowhere to go";

		if (parent.States == null || parent.EventLinks == null) return "its parent cannot hold the nodes";
		return null;
	}

	private void Flatten(Graph subgraph, Graph parent) {
		var inner = subgraph.States.Select(s => s.Element).ToList();
		var initial = inner.First(GraphTopology.IsInitial);
		var exit = GraphTopology.LinksFrom(subgraph).FirstOrDefault();

		// The nodes move first, keeping the subgraph's name on them: after this they sit beside
		// their parent's own nodes and "Wait" on its own is no longer enough to tell them apart.
		foreach (var node in inner) {
			Rename(node, subgraph.Name);
			Reparent(node, parent);
			parent.States.Add(new(node));
		}
		subgraph.States.Clear();

		// Whatever entered the subgraph now enters the node it would have started at. The link has
		// to leave the subgraph's own list as well, or removing the subgraph deletes it.
		GraphTopology.EnsureEntryPoint(initial, Vm);
		var arriving = GraphTopology.LinksInto(subgraph)
			.Union(subgraph.InputLinks ?? [])
			.ToList();
		foreach (var link in arriving) {
			link.Destination = new(initial);
			link.DestEntryPointIndex = GraphTopology.EntriesOf(initial).FirstOrDefault().Index;
			subgraph.InputLinks?.Remove(link);
			if (InputLinksOf(initial) is { } list && !list.Contains(link)) list.Add(link);
		}

		// Exactly one node is initial in a graph, so the inner one only stays initial if the
		// subgraph itself was.
		GraphTopology.SetInitial(initial, subgraph.Initial);

		foreach (var link in (subgraph.EventLinks ?? []).ToList()) {
			link.Parent = parent;

			// "Out of the subgraph" was the subgraph node's own exit; now it is wherever that exit
			// went, and if it went nowhere it keeps returning, one level further out.
			if (GraphTopology.IsTerminator(link) &&
				GraphTopology.ExitTypeOf(link) == GraphTopology.LinkExit.OuterGraph && exit != null) {
				link.Destination = exit.Destination;
				link.DestEntryPointIndex = exit.DestEntryPointIndex;
				if (exit.Destination?.Element is { } destination) InputLinksOf(destination)?.Add(link);
			}

			parent.EventLinks.Add(link);
		}
		subgraph.EventLinks?.Clear();

		// The subgraph node's own exit has been absorbed into the links above and its source is
		// about to stop existing.
		if (exit != null) {
			parent.EventLinks.Remove(exit);
			if (exit.Destination?.Element is { } destination) InputLinksOf(destination)?.Remove(exit);
			subgraph.OutputLinks?.Remove(exit);
			Vm.RemoveElement(exit);
		}

		var name = subgraph.Name ?? subgraph.Id.ToString();
		Vm.RemoveElement(subgraph);
		Logger.Log(LogLevel.Info, $"Flattened '{name}' into '{parent.Name}' — {inner.Count} node(s) moved");
	}

	private static void Rename(VmElement node, string? prefix) {
		if (string.IsNullOrEmpty(prefix) || node is not INamedElement named) return;
		named.Name = string.IsNullOrEmpty(named.Name) ? prefix : $"{prefix}: {named.Name}";
	}

	private static void Reparent(VmElement node, Graph parent) {
		switch (node) {
			case State state: state.Parent = parent; break;
			case Branch branch: branch.Parent = new(parent); break;
			case Graph graph: graph.Parent = new(parent); break;
			case Talking talking: talking.Parent = parent; break;
		}
	}

	private static List<GraphLink>? InputLinksOf(VmElement node) => node switch {
		IGraphElement element => element.InputLinks,
		Talking talking => talking.InputLinks,
		Speech speech => speech.InputLinks,
		_ => null
	};
}
