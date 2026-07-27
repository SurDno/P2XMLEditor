using System.Collections.Generic;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions.Actions.Position;

[Function("Position.GetAreaType")]
public class PositionGetAreaTypeFunction : VmFunction {
	public override FunctionReturnType ReturnType => FunctionReturnType.Int;
	public override int ParamCount => 0;
	public override List<string>? GetParamStrings() => null;
	public PositionGetAreaTypeFunction(VirtualMachine vm, string[] parameters) {
	}
}
