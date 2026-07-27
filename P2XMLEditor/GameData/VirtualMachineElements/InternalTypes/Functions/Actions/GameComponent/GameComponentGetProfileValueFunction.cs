using System.Collections.Generic;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions.Actions.GameComponent;

[Function("GameComponent.GetProfileValue")]
public class GameComponentGetProfileValueFunction(
	VirtualMachine vm,
	string[] parameters)
	: VmFunction {
	public override FunctionReturnType ReturnType => FunctionReturnType.String;
	public override int ParamCount => 1;
	public FunctionSourceParam<string>? ProfileName { get; } = FunctionSourceParam<string>.Read(parameters[0], vm);
	public override List<string>? GetParamStrings() => [ProfileName?.Write() ?? ""];
}
