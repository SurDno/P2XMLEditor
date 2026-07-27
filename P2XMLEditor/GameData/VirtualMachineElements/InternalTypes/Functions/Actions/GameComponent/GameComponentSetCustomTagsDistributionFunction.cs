using System.Collections.Generic;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions.Actions.GameComponent;

[Function("GameComponent.SetCustomTagsDistribution")]
public class GameComponentSetCustomTagsDistributionFunction(
	VirtualMachine vm,
	string[] parameters)
	: VmFunction {
	public override FunctionReturnType ReturnType => FunctionReturnType.Void;
	public override int ParamCount => 3;
	public FunctionSourceParam<string>? Context { get; } = FunctionSourceParam<string>.Read(parameters[0], vm);
	public FunctionSourceParam<string>? Distribution { get; } = FunctionSourceParam<string>.Read(parameters[1], vm);
	public FunctionSourceParam<string>? Component { get; } = FunctionSourceParam<string>.Read(parameters[2], vm);
	public override List<string>? GetParamStrings() => [Context?.Write(), Distribution?.Write(), Component?.Write()];
}
