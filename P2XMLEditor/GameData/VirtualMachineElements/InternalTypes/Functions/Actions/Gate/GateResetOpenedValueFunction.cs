using System.Collections.Generic;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.Enums;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions.Actions.Gate;

[Function("Gate.ResetOpenedValue")]
public class GateResetOpenedValueFunction(VirtualMachine vm, string[] parameters)
	: VmFunction {
	public override FunctionReturnType ReturnType => FunctionReturnType.Void;
	public override int ParamCount => 1;
	public FunctionSourceParam<Priority> Priority { get; } = FunctionSourceParam<Priority>.Read(parameters[0], vm);
	public override List<string>? GetParamStrings() => [Priority.Write()];
}
