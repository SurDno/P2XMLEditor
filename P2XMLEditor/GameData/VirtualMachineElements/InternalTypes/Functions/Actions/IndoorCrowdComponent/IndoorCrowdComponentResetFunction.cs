using System.Collections.Generic;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions.Actions.IndoorCrowdComponent;

[Function("IndoorCrowdComponent.Reset")]
public class IndoorCrowdComponentResetFunction : VmFunction {
	public override FunctionReturnType ReturnType => FunctionReturnType.Void;
	public override int ParamCount => 0;
	public override List<string>? GetParamStrings() => null;
	public IndoorCrowdComponentResetFunction(VirtualMachine vm, string[] parameters) {
	}
}
