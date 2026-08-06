using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.GameData.VirtualMachineElements.Enums;
using P2XMLEditor.GameData.VirtualMachineElements.Interfaces;
using P2XMLEditor.GameData.VirtualMachineElements.Placeholders;
using P2XMLEditor.Logging;

namespace P2XMLEditor.Suggestions.Refactoring;

[Refactoring("Refactor/Graphs/Merge sequential single-action states"),
 SuppressMessage("ReSharper", "UnusedType.Global")]
public class MergeSequentialStates(VirtualMachine vm) : Suggestion(vm) {
	public override void Execute() {
		var allLinks = Vm.GetElementsByType<GraphLink>().ToList();

		int mergedCount = 0;

		foreach (var link in allLinks) {
			if (!Vm.ElementsById.ContainsKey(link.Id)) continue; // Already deleted in this pass

			// Immediate unconditional link
			if (link.Event != null) continue;
			if (link.EventObject != null) continue;

			if (link.Source?.Element is not State stateA) continue;
			if (link.Destination?.Element is not State stateB) continue;

			// Verify 1 output for A, 1 input for B
			var aOuts = stateA.OutputLinks ?? [];
			if (aOuts.Count != 1) continue;

			var bIns = stateB.InputLinks ?? [];
			if (bIns.Count != 1) continue;
			
			if (stateB.Initial) continue;

			// Verify EntryPoints
			if (stateA.EntryPoints?.Count != 1) continue;
			if (stateB.EntryPoints?.Count != 1) continue;

			var epA = stateA.EntryPoints[0];
			var epB = stateB.EntryPoints[0];
			if (epA.ActionLine is not { } alA) continue;
			if (epB.ActionLine is not { } alB) continue;

			// Verify No Loop
			if (alA.ActionLineType == ActionLineType.Loop) continue;
			if (alB.ActionLineType == ActionLineType.Loop) continue;

			// We can merge!
			// 1. Move actions from B to A
			if (alB.Actions != null) {
				alA.Actions ??= new List<VmEither<Action, ActionLine>>();
				foreach (var actRef in alB.Actions) {
					UpdateLocalContextRecursively(actRef, stateA);
					alA.Actions.Add(actRef);
				}
				
				// Re-index all actions in the merged ActionLine
				for (int i = 0; i < alA.Actions.Count; i++) {
					if (alA.Actions[i].Element is Action actElem) {
						actElem.OrderIndex = i;
					} else if (alA.Actions[i].Element is ActionLine actLineElem) {
						actLineElem.OrderIndex = i;
					}
				}

				alB.Actions.Clear();
			}

			// 2. Remove the deleted link A->B from stateA.OutputLinks
			stateA.OutputLinks?.Remove(link);

			// 3. Rewire outgoing links of B to exit from A
			var bOuts = stateB.OutputLinks ?? [];
			foreach (var bOut in bOuts) {
				bOut.Source = new VmEither<Graph, Branch, Speech, State, GraphPlaceholder>(stateA);
				bOut.SourceExitPointIndex = 0;
				// Add the rewired link to stateA's OutputLinks
				stateA.OutputLinks?.Add(bOut);
			}

			// Prevent Vm.RemoveElement from recursively deleting the output links we just rescued
			stateB.OutputLinks?.Clear();

			// 4. Delete State B (its OnDestroy will automatically delete the A->B link, the EntryPoint, and ActionLine)
			Vm.RemoveElement(stateB);

			string graphContext = stateB.Parent is Graph parentGraph ? $" within graph '{parentGraph.Name}'" : "";
			Logger.Log(LogLevel.Info,
				$"Merged sequential state '{stateB.Name}' into state '{stateA.Name}'{graphContext}");

			mergedCount++;
		}

		Logger.Log(LogLevel.Info, $"Completed: Merged {mergedCount} sequential single-action states.");
	}
	private void UpdateLocalContextRecursively(VmEither<Action, ActionLine> actRef, State newState) {
		var newContext = new VmEither<State, Graph, Branch, Talking, Speech>(newState);
		if (actRef.Element is Action actElem) {
			actElem.LocalContext = newContext;
		} else if (actRef.Element is ActionLine actLineElem) {
			actLineElem.LocalContext = newContext;
			if (actLineElem.Actions != null) {
				foreach (var nested in actLineElem.Actions) {
					UpdateLocalContextRecursively(nested, newState);
				}
			}
		}
	}
}
