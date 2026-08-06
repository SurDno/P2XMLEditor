using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.GameData.VirtualMachineElements.Interfaces;
using P2XMLEditor.Logging;

namespace P2XMLEditor.Suggestions.Refactoring;
/* todo: rework
[Refactoring("Refactor/Graphs/Flatten strictly safe subgraphs"), SuppressMessage("ReSharper", "UnusedType.Global")]
public class FlattenSubgraphs(VirtualMachine vm) : Suggestion(vm) {
	public override void Execute() {
		var graphs = Vm.GetElementsByType<Graph>().ToList();
		var allLinks = Vm.GetElementsByType<GraphLink>().ToList();

		int flattenedCount = 0;

		foreach (var sg in graphs) {
			// Subgraph must have at least 1 State
			if (sg.States == null || sg.States.Count == 0) continue;

			// Subgraph must be nested inside another Graph (not a root graph)
			if (sg.Parent.Element is not Graph parentGraph) continue;

			// Check constraints to ensure safe flattening
			bool isSafe = true;
			if (sg.InputParams?.Count > 0) isSafe = false;
			if (sg.SubstituteGraph != null) isSafe = false;

			// Do not flatten if this subgraph is used as a substitute reference anywhere else
			if (graphs.Any(g => g.SubstituteGraph != null && g.SubstituteGraph.Value.Element == sg)) {
				isSafe = false;
			}

			if (sg.EntryPoints != null) {
				foreach (var ep in sg.EntryPoints) {
					var actionLine = ep.ActionLine;
					if (actionLine?.Actions?.Count > 0) isSafe = false;
				}
			}

			// Must have exactly 1 EntryPoint for deterministic wiring
			if (sg.EntryPoints?.Count != 1) isSafe = false;

			// Check OutputLinks of the subgraph node itself
			GraphLink? unconditionalOutputLink = null;
			if (isSafe && sg.OutputLinks != null) {
				if (sg.OutputLinks.Count > 1) {
					isSafe = false;
				} else if (sg.OutputLinks.Count == 1) {
					var outLink = sg.OutputLinks[0];
					if (outLink.Event != null) {
						isSafe = false; // Conditional link on the Subgraph node
					} else {
						unconditionalOutputLink = outLink;
					}
				}
			}

			if (!isSafe) continue;

			var sgEntryPoint = sg.EntryPoints[0];
			object? targetInnerStateForEntry = null;
			int targetInnerEntryPointIndex = 0; // Default to 0 for initial entry point

			// 1. Reparent inner states to the parent graph and find target entry state
			foreach (var innerStateWrapper in sg.States.ToList()) {
				var innerState = innerStateWrapper.Element;
				
				// Re-parent and rename
				if (innerState is INamedElement namedElement && !string.IsNullOrEmpty(sg.Name)) {
					namedElement.Name = $"{sg.Name}: {namedElement.Name}";
				}

				if (innerState is State st) { st.Parent = parentGraph; parentGraph.States.Add(st); if (st.Initial) { targetInnerStateForEntry = st; st.Initial = false; } }
				else if (innerState is Graph gr) { gr.Parent = parentGraph; parentGraph.States.Add(gr); if (gr.Initial) { targetInnerStateForEntry = gr; gr.Initial = false; } }
				else if (innerState is Branch br) { br.Parent = parentGraph; parentGraph.States.Add(br); if (br.Initial) { targetInnerStateForEntry = br; br.Initial = false; } }
				else if (innerState is Talking tk) { tk.Parent = parentGraph; parentGraph.States.Add(tk); if (tk.Initial) { targetInnerStateForEntry = tk; tk.Initial = false; } }
			}
			
			// Prevent Vm.RemoveElement from recursively deleting the states we just rescued
			sg.States.Clear();

			// 2. Rewire all incoming links to point to the inner state
			if (targetInnerStateForEntry != null) {
				foreach (var link in allLinks) {
					if (link.Destination?.Element == sg) {
						if (targetInnerStateForEntry is State st3) link.Destination = st3;
						else if (targetInnerStateForEntry is Graph gr3) link.Destination = gr3;
						else if (targetInnerStateForEntry is Branch br3) link.Destination = br3;
						else if (targetInnerStateForEntry is Talking tk3) link.Destination = tk3;
						link.DestEntryPointIndex = targetInnerEntryPointIndex;
					}
				}

				// 2b. Transfer Initial flag if the subgraph was the initial state
				if (sg.Initial) {
					if (targetInnerStateForEntry is State stInit) stInit.Initial = true;
					else if (targetInnerStateForEntry is Graph grInit) grInit.Initial = true;
					else if (targetInnerStateForEntry is Branch brInit) brInit.Initial = true;
					else if (targetInnerStateForEntry is Talking tkInit) tkInit.Initial = true;
				}
			}

			// 3. Move EventLinks from subgraph to parent and rewire exits
			if (sg.EventLinks != null) {
				foreach (var evLink in sg.EventLinks.ToList()) {
					evLink.Parent = parentGraph;
					
					// Rewire exit links (OuterGraph)
					if (evLink.Destination == null && evLink.DestEntryPointIndex == 1 /* OuterGraph ) {
						if (unconditionalOutputLink != null) {
							evLink.Destination = unconditionalOutputLink.Destination;
							evLink.DestEntryPointIndex = unconditionalOutputLink.DestEntryPointIndex;
						}
					}
					
					parentGraph.EventLinks.Add(evLink);
				}
				sg.EventLinks.Clear();
			}

			// 4. Remove Subgraph from VM (this will recursively delete its EntryPoints and OutputLinks)
			Vm.RemoveElement(sg);
			Logger.Log(LogLevel.Info, $"Flattened subgraph '{sg.Name ?? sg.Id.ToString()}' into parent '{parentGraph.Name ?? parentGraph.Id.ToString()}'");
			flattenedCount++;
		}
		
		Logger.Log(LogLevel.Info, $"Completed: Flattened {flattenedCount} strictly safe subgraphs.");
	}
}
*/