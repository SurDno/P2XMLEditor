using System.Diagnostics.CodeAnalysis;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.Helper;

namespace P2XMLEditor.Suggestions.Refactoring;

[Refactoring("Refactor/Action Lines/Remove empty action lines"), SuppressMessage("ReSharper", "UnusedType.Global")]
public class RemoveEmptyActionLines(VirtualMachine vm) : Suggestion(vm) {
	public override void Execute() {
		var entryPoints = Vm.GetElementsByType<EntryPoint>();
		
		foreach (var entryPoint in entryPoints) {
			var actionLine = entryPoint.ActionLine;
			if (actionLine == null) continue;
			if (vm.GetNullableElement(actionLine.Id) == null) continue;
			if (actionLine.Actions != null && actionLine.Actions.Count != 0) continue;
			Vm.RemoveElement(actionLine);
			entryPoint.ActionLine = null;
		}
	}
}
