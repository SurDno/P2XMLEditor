using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.GameData.VirtualMachineElements.Enums;

namespace P2XMLEditor.Suggestions.Refactoring;

[Refactoring("Events/Remove messages from non-manual events"), SuppressMessage("ReSharper", "UnusedType.Global")]
public class RemoveMessagesFromNonManualEvents(VirtualMachine vm) : Suggestion(vm) {
	public override void Execute() {
		var events = Vm.GetElementsByType<Event>();
		
		foreach (var ev in events) {
			if (ev.Manual == null || ev.Manual.Value) continue;

			ev.RawMessagesInfo = [];
		}
	}
}