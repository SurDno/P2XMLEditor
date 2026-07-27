using System.Collections.Generic;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions.Actions.GameComponent;

[Function("GameComponent.SetProfileFloatValue")]
public class GameComponentSetProfileFloatValueFunction(
	VirtualMachine vm,
	string[] parameters)
	: VmFunction {
	public override FunctionReturnType ReturnType => FunctionReturnType.Void;
	public override int ParamCount => 2;
	public FunctionSourceParam<string>? ProfileName { get; } = FunctionSourceParam<string>.Read(parameters[0], vm);
	public FunctionSourceParam<float>? Value { get; } = FunctionSourceParam<float>.Read(parameters[1], vm);
	public override List<string>? GetParamStrings() => [ProfileName.Write(), Value.Write()];
}
