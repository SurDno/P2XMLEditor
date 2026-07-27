using System.Collections.Generic;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions.Actions.OutdoorCrowdComponent;

[Function("OutdoorCrowdComponent.Reset")]
public class OutdoorCrowdComponentResetFunction : VmFunction {
	public override FunctionReturnType ReturnType => FunctionReturnType.Void;
	public override int ParamCount => 0;
	public override List<string>? GetParamStrings() => null;
	public OutdoorCrowdComponentResetFunction(VirtualMachine vm, string[] parameters) {
	}
}
