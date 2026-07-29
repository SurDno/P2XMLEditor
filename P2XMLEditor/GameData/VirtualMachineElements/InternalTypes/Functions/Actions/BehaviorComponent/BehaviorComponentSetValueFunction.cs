using P2XMLEditor.GameData.Enums;
using System.Collections.Generic;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions.Actions.BehaviorComponent;

[Function("BehaviorComponent.SetValue")]
public class BehaviorComponentSetValueFunction(VirtualMachine vm, string[] parameters)
	: VmFunction {
	public override VmType ReturnType => VmType.Void;
	public override int ParamCount => 2;
	public FunctionSourceParam<string> VariableName { get; } = FunctionSourceParam<string>.Read(parameters[0], vm);
	public FunctionSourceParam<EntityRef> Value { get; } = FunctionSourceParam<EntityRef>.Read(parameters[1], vm);
	public override List<string>? GetParamStrings() => [VariableName.Write(), Value.Write()];
}