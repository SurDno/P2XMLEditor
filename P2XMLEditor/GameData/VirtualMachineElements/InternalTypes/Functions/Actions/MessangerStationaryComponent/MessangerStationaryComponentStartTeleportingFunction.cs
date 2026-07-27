using System.Collections.Generic;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions.Actions.MessangerStationaryComponent;

[Function("MessangerStationaryComponent.StartTeleporting")]
public class MessangerStationaryComponentStartTeleportingFunction : VmFunction {
	public override FunctionReturnType ReturnType => FunctionReturnType.Void;
	public override int ParamCount => 0;
	public override List<string>? GetParamStrings() => null;
	public MessangerStationaryComponentStartTeleportingFunction(VirtualMachine vm, string[] parameters) {
	}
}
