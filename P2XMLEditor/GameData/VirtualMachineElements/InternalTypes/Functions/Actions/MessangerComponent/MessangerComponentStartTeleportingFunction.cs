using System.Collections.Generic;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions.Actions.MessangerComponent;

[Function("MessangerComponent.StartTeleporting")]
public class MessangerComponentStartTeleportingFunction : VmFunction {
	public override FunctionReturnType ReturnType => FunctionReturnType.Void;
	public override int ParamCount => 0;
	public override List<string>? GetParamStrings() => null;
	public MessangerComponentStartTeleportingFunction(VirtualMachine vm, string[] parameters) {
	}
}
