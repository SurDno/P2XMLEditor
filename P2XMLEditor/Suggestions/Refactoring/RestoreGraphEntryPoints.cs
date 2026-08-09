using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.Logging;

namespace P2XMLEditor.Suggestions.Refactoring;

/// <summary>
/// Gives an entry point back to every graph that has links arriving at it and none to arrive at.
///
/// A link stores DestEntryPointIndex — 0 on all 38286 of them — and
/// <c>VMState.GetLocalContextVariables</c> checks that index against the node's own entry points
/// for every input link, logging "Wrong entry point index" when it does not fit. A graph with
/// links and no entry point therefore logs one line per link every time its local context is
/// resolved, which is while conditions and expressions on it are being updated.
///
/// The shipped data never has that shape: of the 866 graphs in PathologicSandbox and 5 in
/// MarbleNest with no entry point, every one has zero input links. So this repairs two things —
/// a graph stripped by an earlier, wrong version of
/// <see cref="RemoveUnreadGraphEntryPoints"/>, and one built by hand without an entry point and
/// then linked to.
///
/// The entry point it creates is the shape the data uses: named "Default", which is what all 7989
/// of them are called, and with no action line, which is true of 7134 of the Sandbox's 7153 — a
/// graph's entry actions never run anyway, since a link entering a graph is applied to the
/// subgraph's initial state.
/// </summary>
[Refactoring("Refactor/Graphs/Restore missing graph entry points"),
 SuppressMessage("ReSharper", "UnusedType.Global")]
public class RestoreGraphEntryPoints(VirtualMachine vm) : Suggestion(vm) {
	public override void Execute() {
		var inbound = new Dictionary<ulong, int>();
		foreach (var link in Vm.GetElementsByType<GraphLink>())
			if (link.Destination?.Element is { } destination)
				inbound[destination.Id] = inbound.GetValueOrDefault(destination.Id) + 1;

		var restored = 0;
		var links = 0;

		foreach (var graph in Vm.GetElementsByType<Graph>().ToList()) {
			if (graph.EntryPoints is { Count: > 0 }) continue;
			if (!inbound.TryGetValue(graph.Id, out var arriving) || arriving == 0) continue;

			var point = VmElement.CreateDefault<EntryPoint>(Vm, graph);
			point.Name = "Default";

			// CreateDefault gives it a line to run; a graph's never runs, and the data agrees —
			// almost none of them carry one.
			if (point.ActionLine is { } line) {
				point.ActionLine = null;
				Vm.RemoveElement(line);
			}

			(graph.EntryPoints ??= []).Add(point);
			restored++;
			links += arriving;

			Logger.Log(LogLevel.Info,
				$"Restored the entry point of '{graph.Name}' ({graph.Id}), which {arriving} link(s) arrive at");
		}

		Logger.Log(LogLevel.Info,
			$"Restored {restored} graph entry point(s), silencing \"Wrong entry point index\" for {links} link(s).");
	}
}
