using P2XMLEditor.GameData.Enums;
using System.Collections.Generic;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions.Actions.BehaviorComponent;

[Function("BehaviorComponent.SetBehaviorForced")]
public class BehaviorComponentSetBehaviorForcedFunction(
	VirtualMachine vm,
	string[] parameters)
	: VmFunction {
	public override VmType ReturnType => VmType.Void;
	public override int ParamCount => 1;
	public FunctionSourceParam<Sample>? Behavior { get; } = FunctionSourceParam<Sample>.Read(parameters[0], vm);
	public override List<string>? GetParamStrings() => [Behavior?.Write() ?? ""];
}