using P2XMLEditor.GameData.Enums;
using System.Collections.Generic;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions.Actions.Position;

[Function("Position.GetBuilding")]
public class PositionGetBuildingFunction : VmFunction {
	public override VmType ReturnType => VmType.EntityRef;
	public override int ParamCount => 0;
	public PositionGetBuildingFunction() {
	}
	public PositionGetBuildingFunction(VirtualMachine vm, string[] parameters) {
	}
	public override List<string>? GetParamStrings() => null;
}