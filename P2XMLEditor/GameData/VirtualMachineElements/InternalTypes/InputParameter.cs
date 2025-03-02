using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.Interfaces;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes;

public class InputParameter(Graph graph, string paramName) : ICommonVariableParameter {
	public Graph Graph { get; } = graph;
	public string ParamName { get; } = paramName;
	public string Id => $"{Graph.Id}_inputparam_{ParamName}";

	public static bool TryParse(string input, VirtualMachine vm, out InputParameter? result) {
		result = null;
		if (!input.Contains("_inputparam_")) return false;

		var parts = input.Split(["_inputparam_"], StringSplitOptions.None);
		if (parts.Length != 2) return false;

		if (!vm.ElementsById.TryGetValue(parts[0], out var element) || element is not Graph graph)
			return false;

		result = new InputParameter(graph, parts[1]);
		return true;
	}
}