using P2XMLEditor.GameData.Enums;
using System.Collections.Generic;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions.Actions.LipSync;

[Function("LipSync.Play3DWithParams")]
public class LipSyncPlay3DWithParamsFunction(VirtualMachine vm, string[] parameters)
	: VmFunction {
	public override VmType ReturnType => VmType.Void;
	public override int ParamCount => 3;
	public FunctionSourceParam<Sample>? LipSync { get; } = FunctionSourceParam<Sample>.Read(parameters[0], vm);
	public FunctionSourceParam<float>? MinDistance { get; } = FunctionSourceParam<float>.Read(parameters[1], vm);
	public FunctionSourceParam<float>? MaxDistance { get; } = FunctionSourceParam<float>.Read(parameters[2], vm);
	public override List<string>? GetParamStrings() => [LipSync?.Write() ?? "", MinDistance?.Write() ?? "", MaxDistance?.Write() ?? ""];
}