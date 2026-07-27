using System.Collections.Generic;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.Enums;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions.Actions.Gate;

[Function("Gate.SetMarkedValue")]
public class GateSetMarkedValueFunction(VirtualMachine vm, string[] parameters)
	: VmFunction {
	public override FunctionReturnType ReturnType => FunctionReturnType.Void;
	public override int ParamCount => 2;
	public FunctionSourceParam<Priority> Priority { get; } = FunctionSourceParam<Priority>.Read(parameters[0], vm);
	public FunctionSourceParam<bool> Value { get; } = FunctionSourceParam<bool>.Read(parameters[1], vm);
	public override List<string>? GetParamStrings() => [Priority.Write(), Value.Write()];
}
