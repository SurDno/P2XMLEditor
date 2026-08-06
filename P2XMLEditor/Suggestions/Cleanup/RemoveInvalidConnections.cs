using System.Diagnostics.CodeAnalysis;
using System.Linq;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.GameData.VirtualMachineElements.Placeholders;
using System.Collections.Generic;
using P2XMLEditor.Logging;

namespace P2XMLEditor.Suggestions.Cleanup;

[Cleanup("Cleanup/Graphs/Remove invalid connections"), SuppressMessage("ReSharper", "UnusedType.Global")]
public class RemoveInvalidConnections(VirtualMachine vm) : Suggestion(vm) {
	public override void Execute() {
		var allLinks = Vm.GetElementsByType<GraphLink>().ToList();

		int removedCount = 0;

		foreach (var link in allLinks) {
			bool isInvalid = false;
			string reason = "";

			if (link.Source?.Element is GraphPlaceholder) {
				isInvalid = true;
				reason = "Source is a GraphPlaceholder";
			}

			if (link.Destination == null && link.DestEntryPointIndex > 1) {
				isInvalid = true;
				reason = "Destination is null and DestEntryPointIndex > 1";
			}

			if (isInvalid) {
				string context = "Unknown Context";
				if (link.Parent.Element is Graph parentGraph) {
					context = $"graph '{parentGraph.Name}'";
				}
				
				Logger.Log(LogLevel.Info, $"Removed invalid connection in {context}. Reason: {reason}");
				Vm.RemoveElement(link);
				removedCount++;
			}
		}

		Logger.Log(LogLevel.Info, $"Completed: Removed {removedCount} invalid connections.");
	}
}
