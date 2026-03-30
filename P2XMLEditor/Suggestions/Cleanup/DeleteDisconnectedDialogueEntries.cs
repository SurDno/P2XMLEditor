using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.Logging;

namespace P2XMLEditor.Suggestions.Cleanup;

[Cleanup("References/Dialogues/Delete disconnected entries"), SuppressMessage("ReSharper", "UnusedType.Global")]
public class DeleteDisconnectedDialogueEntries(VirtualMachine vm) : Suggestion(vm) {
	public override void Execute() {
	   var dialogs = Vm.GetElementsByType<Talking>();
	   
	   foreach (var dialog in dialogs) {
		  var states = dialog.States;

		  var start = states.FirstOrDefault(s => s.Element is Branch { Initial: true }).Element ??
					  states.FirstOrDefault(s => s.Element is Speech { Initial: true }).Element;
		  
		  if (start == null) {
			  Logger.Log(LogLevel.Info, $"Talking graph '{dialog.Name}' has no initial state");
			  continue;
		  }

		  var reachable = GetReachableStates(start);
		  
		  var allStates = states.Select(s => s.Element).ToHashSet();
		  var disconnected = allStates.Except(reachable).ToList();
		  
		  if (disconnected.Count > 0) {
			  Logger.Log(LogLevel.Info, $"Found {disconnected.Count} disconnected entries in '{dialog.Name}':");
			  foreach (var state in disconnected) {
				  var typeName = state switch {
					  Speech => "Speech",
					  Branch => "Branch",
					  _ => state.GetType().Name
				  };
				  Logger.Log(LogLevel.Info, $"  - {typeName}: '{state}' (GUID: {state.Id})");
				  
				  vm.RemoveElement(state);
			  }
		  }
	   }
	}
	
	private HashSet<VmElement> GetReachableStates(VmElement start) {
		var reachable = new HashSet<VmElement>();
		var toVisit = new Queue<VmElement>();
		
		toVisit.Enqueue(start);
		reachable.Add(start);
		
		while (toVisit.Count > 0) {
			var current = toVisit.Dequeue();
			
			var destinations = GetDestinations(current);
			
			foreach (var dest in destinations) {
				if (dest != null && !reachable.Contains(dest)) {
					reachable.Add(dest);
					toVisit.Enqueue(dest);
				}
			}
		}
		
		return reachable;
	}
	
	private IEnumerable<VmElement> GetDestinations(VmElement current) {
		var destinations = new List<VmElement>();
		
		switch (current) {
			case Speech speech:
				for (int i = 0; i < speech.Replies.Count; i++) {
					var replyLink = speech.OutputLinks.FirstOrDefault(l => l.SourceExitPointIndex == i);
					var destination = replyLink?.Destination?.Element;
					if (destination != null) destinations.Add(destination);
				}
	
				var afterExitLink = speech.OutputLinks.FirstOrDefault(l => l.Event == null && l.SourceExitPointIndex == -1);
				var afterExitDestination = afterExitLink?.Destination?.Element;
				if (afterExitDestination != null) destinations.Add(afterExitDestination);
				
				break;
	
			case Branch branch:
				int exitPointCount = branch.BranchConditions.Count + 1;
	
				for (int i = 0; i < exitPointCount; i++) {
					var branchLink = branch.OutputLinks.FirstOrDefault(l => l.SourceExitPointIndex == i);
					var branchDestination = branchLink?.Destination?.Element;
					if (branchDestination != null) destinations.Add(branchDestination);
				}
				break;
		}
		
		return destinations;
	}
}