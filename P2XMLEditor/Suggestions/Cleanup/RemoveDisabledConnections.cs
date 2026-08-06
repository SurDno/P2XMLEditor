using System.Diagnostics.CodeAnalysis;
using System.Linq;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.Logging;

namespace P2XMLEditor.Suggestions.Cleanup;

[Cleanup("Cleanup/Graphs/Remove disabled connections"), SuppressMessage("ReSharper", "UnusedType.Global")]
public class RemoveDisabledConnections(VirtualMachine vm) : Suggestion(vm) {
	public override void Execute() {
		var allLinks = Vm.GetElementsByType<GraphLink>().ToList();

		int removedCount = 0;

		foreach (var link in allLinks) {
			if (!link.Enabled) {
				string context = "Unknown Context";
				if (link.Parent.Element is Graph parentGraph) {
					context = $"graph '{parentGraph.Name}'";
				}
				
				string sourceContext = "Unknown Source";
				if (link.Source?.Element is P2XMLEditor.GameData.VirtualMachineElements.Interfaces.IGraphElement sourceElement) {
					sourceContext = $"'{sourceElement.Name}' ({sourceElement.GetType().Name})";
				}
				
				Logger.Log(LogLevel.Info, $"Removed disabled connection from {sourceContext} in {context}");
				Vm.RemoveElement(link);
				removedCount++;
			}
		}
		
		Logger.Log(LogLevel.Info, $"Completed: Removed {removedCount} disabled connection(s).");
	}
}
