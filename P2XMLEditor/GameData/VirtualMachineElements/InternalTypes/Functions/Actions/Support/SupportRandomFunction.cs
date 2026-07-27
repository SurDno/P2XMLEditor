using System.Collections.Generic;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions.Actions.Support;

[Function("Support.Random")]
public class SupportRandomFunction : VmFunction {
	public override FunctionReturnType ReturnType => FunctionReturnType.Float;
	public override int ParamCount => 0;
	public override List<string>? GetParamStrings() => null;
	public SupportRandomFunction(VirtualMachine vm, string[] parameters) {
	}
}
