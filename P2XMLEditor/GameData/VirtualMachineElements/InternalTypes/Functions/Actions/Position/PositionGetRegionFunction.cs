using System.Collections.Generic;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions.Actions.Position;

[Function("Position.GetRegion")]
public class PositionGetRegionFunction : VmFunction {
	public override FunctionReturnType ReturnType => FunctionReturnType.Entity;
	public override int ParamCount => 0;
	public override List<string>? GetParamStrings() => null;
	public PositionGetRegionFunction(VirtualMachine vm, string[] parameters) {
	}
}
