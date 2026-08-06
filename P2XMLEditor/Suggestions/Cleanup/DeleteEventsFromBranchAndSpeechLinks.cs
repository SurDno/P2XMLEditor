using System.Diagnostics.CodeAnalysis;
using System.Linq;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.Helper;
using P2XMLEditor.Logging;

namespace P2XMLEditor.Suggestions.Cleanup;

[Cleanup("References/Events/Delete events from Branch and Speech links"), SuppressMessage("ReSharper", "UnusedType.Global")]
public class DeleteEventsFromBranchAndSpeechLinks(VirtualMachine vm) : Suggestion(vm) {
	public override void Execute() {
		var links = Vm.GetElementsByType<GraphLink>()
			.Where(l => l.Event != null || l.EventObject != null)
			.Where(l => l.Source?.Element is Branch || l.Source?.Element is Speech)
			.ToList();

		int removedCount = 0;
		foreach (var link in links) {
			link.Event = null;
			link.EventObject = null;
			
			string context = "Unknown Source";
			if (link.Source?.Element is Branch br) {
				context = $"Branch '{br.Name}'";
			} else if (link.Source?.Element is Speech sp) {
				context = $"Speech '{sp.Text}'";
			}
			
			Logger.Log(LogLevel.Info, $"Deleted event from link originating at {context}");
			removedCount++;
		}
		
		Logger.Log(LogLevel.Info, $"Completed: Deleted events from {removedCount} Branch/Speech links.");
	}

}
