using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.GameData.VirtualMachineElements.Interfaces;
using P2XMLEditor.GameData.VirtualMachineElements.Placeholders;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes;

public class LoopParameter(ActionLine actionLine, bool isIndex, string? listName)  {
	public ActionLine ActionLine { get; } = actionLine;
	public bool IsIndex { get; } = isIndex;
	public string? ListName { get; } = listName;
	public string ParamId => IsIndex ? $"local_{ActionLine.Id}_Loop_Index" : $"local_{ActionLine.Id}_Loop_List_{ListName}_Element";

	public static bool TryParse(string input, VirtualMachine vm, out LoopParameter? result) {
		result = null;
		if (!input.StartsWith("local_") || !input.Contains("_Loop_")) {
			return false;
		}
		var array = input.Split('_');
		if (array.Length >= 4 && ulong.TryParse(array[1], out var result2)) {
			if (!vm.ElementsById.TryGetValue(result2, out var value)) {
				value = vm.Register(new ActionLinePlaceholder(result2));
			}
			if (value is ActionLine actionLine) {
				if (input.EndsWith("_Loop_Index")) {
					result = new LoopParameter(actionLine, isIndex: true, null);
					return true;
				}
				if (input.Contains("_Loop_List_") && input.EndsWith("_Element")) {
					var text = $"local_{result2}_Loop_List_";
					var text2 = "_Element";
					if (input.Length > text.Length + text2.Length) {
						var listName = input.Substring(text.Length, input.Length - text.Length - text2.Length);
						result = new LoopParameter(actionLine, isIndex: false, listName);
						return true;
					}
				}
			}
		}
		return false;
	}
}
