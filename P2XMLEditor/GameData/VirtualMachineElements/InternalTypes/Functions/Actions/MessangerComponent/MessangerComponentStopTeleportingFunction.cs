using P2XMLEditor.GameData.Enums;
using System.Collections.Generic;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions.Actions.MessangerComponent;

[Function("MessangerComponent.StopTeleporting")]
public class MessangerComponentStopTeleportingFunction : VmFunction {
	public override VmType ReturnType => VmType.Void;
	public override int ParamCount => 0;
	public override List<string>? GetParamStrings() => null;
	public MessangerComponentStopTeleportingFunction(VirtualMachine vm, string[] parameters) {
	}
}