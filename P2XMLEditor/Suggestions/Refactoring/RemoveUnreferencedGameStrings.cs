using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes;

namespace P2XMLEditor.Suggestions.Refactoring;

[Refactoring("Refactor/Remove unreferenced GameStrings"), SuppressMessage("ReSharper", "UnusedType.Global")]
public class RemoveUnreferencedGameStrings(VirtualMachine vm) : Suggestion(vm) {
	public override void Execute() {
		var allStrings = Vm.GetElementsByType<GameString>().ToList();
		var usedStrings = new HashSet<GameString>();

		foreach (var speech in Vm.GetElementsByType<Speech>()) {
			if (speech.Text != null) usedStrings.Add(speech.Text);
		}

		foreach (var reply in Vm.GetElementsByType<Reply>()) {
			if (reply.Text != null) usedStrings.Add(reply.Text);
		}

		foreach (var mindMap in Vm.GetElementsByType<MindMap>()) {
			if (mindMap.Title != null) usedStrings.Add(mindMap.Title);
			if (mindMap.TextObjects != null) {
				foreach (var text in mindMap.TextObjects) {
					if (text != null) usedStrings.Add(text);
				}
			}
		}

		foreach (var mindMapNode in Vm.GetElementsByType<MindMapNode>()) {
			if (mindMapNode.NodeNameText != null) usedStrings.Add(mindMapNode.NodeNameText);
			if (mindMapNode.NodeDescriptionText != null) usedStrings.Add(mindMapNode.NodeDescriptionText);
		}

		foreach (var mindMapNodeContent in Vm.GetElementsByType<MindMapNodeContent>()) {
			if (mindMapNodeContent.ContentDescriptionText != null) usedStrings.Add(mindMapNodeContent.ContentDescriptionText);
		}

		foreach (var parameterHolder in Vm.GetElementsByType<ParameterHolder>()) {
			foreach (var param in parameterHolder.StandartParams.Values.Concat(parameterHolder.CustomParams.Values)) {
				if (param.Value is RefValue<GameString> { TypedValue: not null } textRef) {
					usedStrings.Add(textRef.TypedValue);
				}
			}
		}

		int emptyUnusedCount = 0;
		int totalUnusedCount = 0;

		foreach (var gameString in allStrings) {
			if (!usedStrings.Contains(gameString)) {
				totalUnusedCount++;
				var english = gameString.GetText("English");
				var russian = gameString.GetText("Russian");
				bool isEmpty = string.IsNullOrWhiteSpace(english) && string.IsNullOrWhiteSpace(russian);

				if (isEmpty) {
					emptyUnusedCount++;
				} else {
					var englishText = !string.IsNullOrWhiteSpace(english) ? english : russian;
					System.Console.WriteLine($"Removed Unused String {gameString.Id}: {englishText}");
				}
				
				Vm.RemoveElement(gameString);
			}
		}

		System.Console.WriteLine($"Total strings removed: {totalUnusedCount}");
		System.Console.WriteLine($"Empty unused strings: {emptyUnusedCount}");
		System.Console.WriteLine($"Non-empty unused strings: {totalUnusedCount - emptyUnusedCount}");
	}
}
