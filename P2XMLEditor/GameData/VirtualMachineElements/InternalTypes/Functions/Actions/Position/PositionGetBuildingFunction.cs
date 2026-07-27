using System.Collections.Generic;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions.Actions.Position;

[Function("Position.GetBuilding")]
public class PositionGetBuildingFunction : VmFunction {
	public override FunctionReturnType ReturnType => FunctionReturnType.Entity;
	public override int ParamCount => 0;
	public PositionGetBuildingFunction() {
	}
	public PositionGetBuildingFunction(VirtualMachine vm, string[] parameters) {
	}
	public override List<string>? GetParamStrings() => null;
}
