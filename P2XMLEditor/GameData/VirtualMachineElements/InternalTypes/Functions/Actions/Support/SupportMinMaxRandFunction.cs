using P2XMLEditor.GameData.Enums;
using System.Collections.Generic;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions.Actions.Support;

[Function("Support.MinMaxRand")]
public class SupportMinMaxRandFunction(VirtualMachine vm, string[] parameters)
	: VmFunction {
	public override VmType ReturnType => VmType.Single;
	public override int ParamCount => 2;
	public FunctionSourceParam<float>? Min { get; } = FunctionSourceParam<float>.Read(parameters[0], vm);
	public FunctionSourceParam<float>? Max { get; } = FunctionSourceParam<float>.Read(parameters[1], vm);
	public override List<string>? GetParamStrings() => [Min.Write(), Max.Write()];
}