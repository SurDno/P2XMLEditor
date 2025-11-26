using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.GameData.VirtualMachineElements.Interfaces;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes;

public class ParamByName(string name) : ICommonVariableParameter {
	private string Name { get; } = name;
	public string ParamId => Name;

	public static bool Validate(string name, VirtualMachine vm) {
		return vm.GetElementsByType<ParameterHolder>().Any(holder => holder.StandartParams.Any(kvp => kvp.Key == name));
	}
}