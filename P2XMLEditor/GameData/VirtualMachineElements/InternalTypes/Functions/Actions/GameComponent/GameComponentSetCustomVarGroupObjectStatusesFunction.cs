using System.Collections.Generic;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions.Actions.GameComponent;

[Function("GameComponent.SetCustomVarGroupObjectStatuses")]
public class GameComponentSetCustomVarGroupObjectStatusesFunction(
	VirtualMachine vm,
	string[] parameters)
	: VmFunction {
	public override FunctionReturnType ReturnType => FunctionReturnType.Void;
	public override int ParamCount => 6;
	public FunctionSourceParam<string>? GroupName { get; } = FunctionSourceParam<string>.Read(parameters[0], vm);
	public FunctionSourceParam<string>? StatusName { get; } = FunctionSourceParam<string>.Read(parameters[1], vm);
	public FunctionSourceParam<string>? ParamA { get; } = FunctionSourceParam<string>.Read(parameters[2], vm);
	public FunctionSourceParam<string>? ValueA { get; } = FunctionSourceParam<string>.Read(parameters[3], vm);
	public FunctionSourceParam<string>? ParamB { get; } = FunctionSourceParam<string>.Read(parameters[4], vm);
	public FunctionSourceParam<string>? ValueB { get; } = FunctionSourceParam<string>.Read(parameters[5], vm);
	public override List<string>? GetParamStrings() => [GroupName?.Write(), StatusName?.Write(), ParamA?.Write(), ValueA?.Write(), ParamB?.Write(), ValueB?.Write()];
}
