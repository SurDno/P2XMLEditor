using P2XMLEditor.GameData.VirtualMachineElements.Interfaces;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes;

public class GlobalVariableParameter(string paramName) : ICommonVariableParameter {
	public string ParamName { get; } = paramName;
	public string ParamId => ParamName;
	public static bool TryParse(string input, out GlobalVariableParameter? result) {
		result = null;
		if (!input.StartsWith("global_")) {
			return false;
		}
		result = new GlobalVariableParameter(input);
		return true;
	}
}
