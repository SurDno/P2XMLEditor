using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.GameData.VirtualMachineElements.Enums;
using P2XMLEditor.Logging;

namespace P2XMLEditor.Suggestions.Refactoring;

[Refactoring("Events/Remove messages from non-manual events"), SuppressMessage("ReSharper", "UnusedType.Global")]
public class RemoveMessagesFromNonManualEvents(VirtualMachine vm) : Suggestion(vm) {
	public override void Execute() {
		var events = Vm.GetElementsByType<Event>();
		
		int removedCount = 0;
		foreach (var ev in events) {
			if (ev.Manual == null || ev.Manual) continue;

			if (ev.RawMessagesInfo?.Length > 0) {
				ev.RawMessagesInfo = [];
				string context = "";
				if (ev.Parent.Element is P2XMLEditor.GameData.VirtualMachineElements.Interfaces.IGraphElement ge) {
					context = $" on {ge.GetType().Name} '{ge.Name}'";
				}
				Logger.Log(LogLevel.Info, $"Removed messages from non-manual event '{ev.Id}'{context}");
				removedCount++;
			}
		}
		
		Logger.Log(LogLevel.Info, $"Completed: Removed messages from {removedCount} non-manual events.");
	}
}