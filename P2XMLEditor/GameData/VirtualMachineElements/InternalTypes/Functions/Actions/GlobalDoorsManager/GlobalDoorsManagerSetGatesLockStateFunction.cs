using System.Collections.Generic;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.Enums;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;
using P2XMLEditor.Helper;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions.Actions.GlobalDoorsManager;

[Function("GlobalDoorsManager.SetGatesLockState")]
public class GlobalDoorsManagerSetGatesLockStateFunction(
	VirtualMachine vm,
	string[] parameters)
	: VmFunction {
	public override FunctionReturnType ReturnType => FunctionReturnType.Void;
	public override int ParamCount => 4;
	public FunctionSourceParam<string> GatesRootInfo { get; } = FunctionSourceParam<string>.Read(parameters[0], vm);
	public FunctionSourceParam<LockState> LockState { get; } = FunctionSourceParam<LockState>.Read(parameters[1], vm);
	public FunctionSourceParam<string> OperationVolume { get; } = FunctionSourceParam<string>.Read(parameters[2], vm);
	public Priority Priority { get; } = parameters[3].TrimStart('%').Deserialize<Priority>();
	public override List<string>? GetParamStrings() => [GatesRootInfo.Write(), LockState.Write(), OperationVolume.Write(), "%" + Priority.Serialize()];
}
