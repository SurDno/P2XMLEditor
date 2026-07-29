using P2XMLEditor.GameData.Enums;
using System.Collections.Generic;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions.Actions.Position;

[Function("Position.GetRegion")]
public class PositionGetRegionFunction : VmFunction {
	public override VmType ReturnType => VmType.EntityRef;
	public override int ParamCount => 0;
	public override List<string>? GetParamStrings() => null;
	public PositionGetRegionFunction(VirtualMachine vm, string[] parameters) {
	}
}