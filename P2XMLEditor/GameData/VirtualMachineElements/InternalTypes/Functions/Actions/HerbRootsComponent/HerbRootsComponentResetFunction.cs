using System.Collections.Generic;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions.Actions.HerbRootsComponent;

[Function("HerbRootsComponent.Reset")]
public class HerbRootsComponentResetFunction : VmFunction {
	public override FunctionReturnType ReturnType => FunctionReturnType.Void;
	public override int ParamCount => 0;
	public override List<string>? GetParamStrings() => null;
	public HerbRootsComponentResetFunction(VirtualMachine vm, string[] parameters) {
	}
}
