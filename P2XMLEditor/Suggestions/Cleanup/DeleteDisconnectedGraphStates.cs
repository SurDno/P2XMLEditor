using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.GameData.VirtualMachineElements.Helper;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes;
using P2XMLEditor.Logging;
using VmAction = P2XMLEditor.GameData.VirtualMachineElements.Action;

namespace P2XMLEditor.Suggestions.Cleanup;

/// <summary>
/// Deletes nodes an event graph can never reach, the way
/// <see cref="DeleteDisconnectedDialogueEntries"/> does for dialogues.
///
/// A graph is walked from its initial node, plus every node a link arrives at from outside — an
/// event link with no source at all (6009 of them in PathologicSandbox) or one whose source
/// belongs to another graph. Anything the walk does not reach cannot be entered by a transition.
///
/// Both halves of that starting set matter. A node entered by an event link is a root, not merely
/// something the walk might arrive at later: 6255 nodes in PathologicSandbox and 582 in MarbleNest
/// cannot be reached from the initial node at all and are only entered by an event, which is a
/// quarter of every node in the game. Walking from the initial node alone would delete them.
///
/// Reading only the graph's own EventLinks is enough to see every way in, because a link is always
/// stored in the container its destination belongs to — all 34898 links with a destination in the
/// Sandbox and all 3388 in MarbleNest, no exceptions.
///
/// That alone would be wrong, though, and dangerously so: it condemns 1216 nodes in the Sandbox,
/// and most of them are alive. Three things enter a node without a link, and each is a guard here:
///
/// * A <b>Talking</b> is never entered by a link at all — FSMTalkingManager starts it because
///   Speaking.CurrentTalking points at it. All 760 Talkings in the Sandbox are "unreachable" by
///   the walk, and only 120 are even named by a parameter value; the rest are chosen at runtime.
///   Talkings are therefore never touched.
/// * A graph named as another graph's <b>SubstituteGraph</b> is entered through that graph —
///   200 of the Sandbox's unreachable nodes and 13 of MarbleNest's.
/// * A node <b>held by a parameter or named by an action</b> can be entered programmatically:
///   IStateRef values point at states and graphs, so anything mentioned that way stays. No
///   remaining candidate is named this way in either corpus, but that is a fact about the shipped
///   data rather than a rule of the format, and the guard costs one pass.
///
/// What is left is 256 nodes in PathologicSandbox (212 states, 32 branches, 12 subgraphs) and 18
/// in MarbleNest — names like SetDayFour, SetMasterVictor, DisableTrigger and GameOverCycle,
/// which read as work that was disconnected and never removed.
///
/// Deleting a node takes its links, entry points and their action lines with it, through
/// OnDestroy. It does not cascade into a deleted subgraph's own contents: those become orphans of
/// a graph nobody references, which is what the other cleanup passes are for.
/// </summary>
[Cleanup("References/Graphs/Delete disconnected states"), SuppressMessage("ReSharper", "UnusedType.Global")]
public class DeleteDisconnectedGraphStates(VirtualMachine vm) : Suggestion(vm) {
	public override void Execute() {
		var protectedNodes = ProtectedNodes();

		var removed = 0;
		var keptProtected = 0;
		var keptTalking = 0;

		foreach (var graph in Vm.GetElementsByType<Graph>().ToList()) {
			var nodes = GraphTopology.NodesOf(graph);
			if (nodes.Count == 0) continue;

			var reachable = Reachable(graph, nodes);

			foreach (var node in nodes.ToList()) {
				if (reachable.Contains(node.Id)) continue;

				if (node is Talking) { keptTalking++; continue; }
				if (protectedNodes.Contains(node.Id)) { keptProtected++; continue; }

				Logger.Log(LogLevel.Info,
					$"Removed {node.GetType().Name} '{GraphTopology.NameOf(node)}' ({node.Id}) — "
					+ $"nothing in '{graph.Name}' reaches it");
				Vm.RemoveElement(node);
				removed++;
			}
		}

		Logger.Log(LogLevel.Info,
			$"Removed {removed} disconnected node(s). Left alone: {keptTalking} talking(s), which links never "
			+ $"enter, and {keptProtected} node(s) reached by substitution or named by a parameter or action.");
	}

	/// <summary>
	/// Everything the flow can get to: the initial node, anything a link enters from outside the
	/// graph, and everything downstream of those.
	/// </summary>
	private static HashSet<ulong> Reachable(Graph graph, IReadOnlyList<VmElement> nodes) {
		var here = nodes.Select(n => n.Id).ToHashSet();
		var links = GraphTopology.LinksOf(graph);

		var reachable = new HashSet<ulong>();
		var pending = new Queue<ulong>();

		foreach (var node in nodes)
			if (GraphTopology.IsInitial(node) && reachable.Add(node.Id))
				pending.Enqueue(node.Id);

		// A link with no source is subscribed to an event and enters from outside; so is one whose
		// source is a node of some other graph.
		foreach (var link in links) {
			if (link.Destination?.Element is not { } destination || !here.Contains(destination.Id)) continue;
			var source = link.Source?.Element;
			if (source != null && here.Contains(source.Id)) continue;
			if (reachable.Add(destination.Id)) pending.Enqueue(destination.Id);
		}

		while (pending.Count > 0) {
			var current = pending.Dequeue();
			foreach (var link in links) {
				if (link.Source?.Element is not { } source || source.Id != current) continue;
				if (link.Destination?.Element is not { } destination || !here.Contains(destination.Id)) continue;
				if (reachable.Add(destination.Id)) pending.Enqueue(destination.Id);
			}
		}

		return reachable;
	}

	/// <summary>
	/// Nodes something can enter without a link: a substituted graph, and anything an id in the
	/// data points at — a parameter's value, an action's arguments, an expression's operands.
	///
	/// Ids are read out of the serialized forms rather than the typed references, because the
	/// typed ones do not survive the question being asked. An IStateRef value is parsed as a
	/// reference to a State, so a parameter holding a *graph* resolves to nothing at all and the
	/// guard would quietly pass a node it should have protected. Every id is collected instead;
	/// over-protecting costs a node that stays, and the alternative costs one that should not
	/// have gone.
	/// </summary>
	private HashSet<ulong> ProtectedNodes() {
		var protectedNodes = new HashSet<ulong>();

		foreach (var graph in Vm.GetElementsByType<Graph>())
			if (graph.SubstituteGraph?.Element is { } substitute)
				protectedNodes.Add(substitute.Id);

		foreach (var parameter in Vm.GetElementsByType<Parameter>())
			Protect(protectedNodes, parameter.SerializedValue);

		foreach (var action in Vm.GetElementsByType<VmAction>()) {
			Protect(protectedNodes, SafeWrite(() => action.TargetObject.Write()));
			foreach (var argument in action.GetParamStrings() ?? []) Protect(protectedNodes, argument);
		}

		foreach (var expression in Vm.GetElementsByType<Expression>()) {
			Protect(protectedNodes, SafeWrite(() => expression.TargetObject.Write()));
			Protect(protectedNodes, SafeWrite(() => expression.TargetParam?.Write()));
			foreach (var argument in expression.Function?.GetParamStrings() ?? [])
				Protect(protectedNodes, argument);
		}

		return protectedNodes;
	}

	/// <summary>
	/// Every id in one serialized value. The separator is '%' — an object and the thing read off
	/// it — and either half can be an id.
	/// </summary>
	private static void Protect(HashSet<ulong> protectedNodes, string? serialized) {
		if (string.IsNullOrEmpty(serialized)) return;
		foreach (var part in serialized.Split('%'))
			if (ulong.TryParse(part, out var id))
				protectedNodes.Add(id);
	}

	private static string? SafeWrite(System.Func<string?> write) {
		try {
			return write();
		} catch {
			return null;
		}
	}
}
