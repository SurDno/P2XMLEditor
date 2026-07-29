using P2XMLEditor.GameData.Enums;
using System.Collections.Generic;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;
using P2XMLEditor.Helper;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions.Actions.Trigger;

[Function("Trigger.IsContainsObject")]
public class TriggerIsContainsObjectFunction : VmFunction {
	public override VmType ReturnType => VmType.Boolean;
	public override int ParamCount => 1;
	public FunctionSourceParam<EntityRef>? Target { get; }
	public override List<string>? GetParamStrings() => [Target?.Write() ?? ""];
	public TriggerIsContainsObjectFunction(VirtualMachine vm, string[] parameters) {
		Target = FunctionSourceParam<EntityRef>.Read(parameters[0], vm);
	}
}