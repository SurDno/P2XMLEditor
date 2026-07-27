using System.Collections.Generic;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.Enums;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions.Actions.Gate;

[Function("Gate.RemovePicklock")]
public class GateRemovePicklockFunction(VirtualMachine vm, string[] parameters)
	: VmFunction {
	public override FunctionReturnType ReturnType => FunctionReturnType.Void;
	public override int ParamCount => 2;
	public FunctionSourceParam<Priority> Priority { get; } = FunctionSourceParam<Priority>.Read(parameters[0], vm);
	public FunctionSourceParam<EntityRef> Storable { get; } = FunctionSourceParam<EntityRef>.Read(parameters[1], vm);
	public override List<string>? GetParamStrings() => [Priority.Write(), Storable.Write()];
}
