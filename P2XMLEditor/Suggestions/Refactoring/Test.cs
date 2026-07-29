using System;
using System.Diagnostics.CodeAnalysis;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements;

namespace P2XMLEditor.Suggestions.Refactoring;

[Refactoring("Test/Test"), SuppressMessage("ReSharper", "UnusedType.Global")]
public class Test(VirtualMachine vm) : Suggestion(vm) {
	public override void Execute() {
		foreach (var branch in Vm.GetElementsByType<Branch>()) {
			if (branch is { Name: "Talking beginning", OutputLinks.Count: > 1 }) {
				var parent = branch.Parent.Element;
				if (parent is Talking talking) 
					Console.WriteLine(talking.Name);
			}
		}
	}
}
