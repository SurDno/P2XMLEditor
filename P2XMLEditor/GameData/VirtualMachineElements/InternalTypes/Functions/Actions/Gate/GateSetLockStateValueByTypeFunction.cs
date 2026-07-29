using System.Collections.Generic;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.Enums;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions.Actions.Gate;

[Function("Gate.SetLockStateValueByType")]
public class GateSetLockStateValueByTypeFunction(VirtualMachine vm, string[] parameters)
	: VmFunction {
	public override VmType ReturnType => VmType.Void;
	public override int ParamCount => 3;
	public FunctionSourceParam<Priority> Priority { get; } = FunctionSourceParam<Priority>.Read(parameters[0], vm);
	public FunctionSourceParam<LockState> Value { get; } = FunctionSourceParam<LockState>.Read(parameters[1], vm);
	public FunctionSourceParam<bool> IsOutdoor { get; } = FunctionSourceParam<bool>.Read(parameters[2], vm);
	public override List<string>? GetParamStrings() => [Priority.Write(), Value.Write(), IsOutdoor.Write()];
}