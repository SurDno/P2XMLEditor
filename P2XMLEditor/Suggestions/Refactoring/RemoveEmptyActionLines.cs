using System.Diagnostics.CodeAnalysis;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.Helper;
using P2XMLEditor.Logging;

namespace P2XMLEditor.Suggestions.Refactoring;

[Refactoring("Refactor/Action Lines/Remove empty action lines"), SuppressMessage("ReSharper", "UnusedType.Global")]
public class RemoveEmptyActionLines(VirtualMachine vm) : Suggestion(vm) {
	public override void Execute() {
		var entryPoints = Vm.GetElementsByType<EntryPoint>();
		
		int removedCount = 0;
		foreach (var entryPoint in entryPoints) {
			var actionLine = entryPoint.ActionLine;
			if (actionLine == null) continue;
			if (vm.GetNullableElement(actionLine.Id) == null) continue;
			if (actionLine.Actions != null && actionLine.Actions.Count != 0) continue;
			
			string parentInfo = "EntryPoint";
			if (entryPoint.Parent?.Element is P2XMLEditor.GameData.VirtualMachineElements.Interfaces.IGraphElement parentElement) {
				if (parentElement is State st) parentInfo = $"state '{st.Name}'";
				else if (parentElement is Speech sp) parentInfo = $"speech '{sp.Text}'";
				else if (parentElement is Branch br) parentInfo = $"branch '{br.Name}'";
				else if (parentElement is Graph gr) parentInfo = $"graph '{gr.Name}'";
				else parentInfo = $"'{parentElement.Name}' ({parentElement.GetType().Name})";
			}
			
			Vm.RemoveElement(actionLine);
			entryPoint.ActionLine = null;
			Logger.Log(LogLevel.Info, $"Removed empty action line from {parentInfo}");
			removedCount++;
		}
		
		Logger.Log(LogLevel.Info, $"Completed: Removed {removedCount} empty action lines.");
	}
}
