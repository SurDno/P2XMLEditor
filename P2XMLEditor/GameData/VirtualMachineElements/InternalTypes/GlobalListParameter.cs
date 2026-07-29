using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.GameData.VirtualMachineElements.Interfaces;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes;

public class GlobalListParameter(string listName, VmElement? targetElement, ulong targetId)  {
	public string ListName { get; } = listName;
	public VmElement? TargetElement { get; } = targetElement;
	public ulong TargetId { get; } = targetId;
	public string ParamId => $"{ListName}_{TargetId}";
	public static bool TryParse(string input, VirtualMachine vm, out GlobalListParameter? result) {
		result = null;
		if (!input.StartsWith("global_")) {
			return false;
		}
		var num = input.LastIndexOf('_');
		if (num != -1) {
			var listName = input.Substring(0, num);
			if (ulong.TryParse(input.Substring(num + 1), out var result2)) {
				vm.ElementsById.TryGetValue(result2, out var value);
				result = new GlobalListParameter(listName, value, result2);
				return true;
			}
		}
		return false;
	}
}
