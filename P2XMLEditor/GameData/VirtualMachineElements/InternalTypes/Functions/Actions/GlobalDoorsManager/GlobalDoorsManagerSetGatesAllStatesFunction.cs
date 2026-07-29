using System.Collections.Generic;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.Enums;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions.Actions.GlobalDoorsManager;

[Function("GlobalDoorsManager.SetGatesAllStates")]
public class GlobalDoorsManagerSetGatesAllStatesFunction(
	VirtualMachine vm,
	string[] parameters)
	: VmFunction {
	public override VmType ReturnType => VmType.Void;
	public override int ParamCount => 6;
	public FunctionSourceParam<string> GatesRootInfo { get; } = FunctionSourceParam<string>.Read(parameters[0], vm);
	public FunctionSourceParam<GateState> OpeningState { get; } = FunctionSourceParam<GateState>.Read(parameters[1], vm);
	public FunctionSourceParam<LockState> LockState { get; } = FunctionSourceParam<LockState>.Read(parameters[2], vm);
	public FunctionSourceParam<string> GateStatuses { get; } = FunctionSourceParam<string>.Read(parameters[3], vm);
	public FunctionSourceParam<string> OperationVolume { get; } = FunctionSourceParam<string>.Read(parameters[4], vm);
	public FunctionSourceParam<Priority> Priority { get; } = FunctionSourceParam<Priority>.Read(parameters[5], vm);
	public override List<string>? GetParamStrings() => [GatesRootInfo.Write(), OpeningState.Write(), LockState.Write(), GateStatuses.Write(), OperationVolume.Write(), Priority.Write()];
}