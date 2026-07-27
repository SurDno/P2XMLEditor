using System.Collections.Generic;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions.Actions.Interactive;

[Function("Interactive.EnableComponent")]
public class InteractiveEnableComponentFunction : VmFunction {
	public override FunctionReturnType ReturnType => FunctionReturnType.Void;
	public override int ParamCount => 1;
	public FunctionSourceParam<bool>? Enabled { get; }
	public override List<string>? GetParamStrings() => [Enabled?.Write() ?? ""];
	public InteractiveEnableComponentFunction(VirtualMachine vm, string[] parameters) {
		Enabled = FunctionSourceParam<bool>.Read(parameters[0], vm);
	}
}
