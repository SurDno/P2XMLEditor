using System.Diagnostics.CodeAnalysis;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.Logging;

namespace P2XMLEditor.Suggestions.Refactoring;

[Refactoring("Refactor/Parameter Holders/Remove empty GUID references"),
 SuppressMessage("ReSharper", "UnusedType.Global")]
public class RemoveEmptyGuidReferences(VirtualMachine vm) : Suggestion(vm) {
	public override void Execute() {
		int count = 0;
		foreach (var item in Vm.GetElementsByType<GameObject>()) {
			if (item.EngineTemplateId == "00000000000000000000000000000000") {
				item.EngineTemplateId = null;
				Logger.Log(LogLevel.Info, $"Removed empty EngineTemplateId from GameObject '{item.Name}'");
				count++;
			}
			if (item.EngineBaseTemplateId == "00000000000000000000000000000000") {
				item.EngineBaseTemplateId = null;
				Logger.Log(LogLevel.Info, $"Removed empty EngineBaseTemplateId from GameObject '{item.Name}'");
				count++;
			}
		}
		Logger.Log(LogLevel.Info, $"Completed: Removed {count} empty GUID references.");
	}
}
