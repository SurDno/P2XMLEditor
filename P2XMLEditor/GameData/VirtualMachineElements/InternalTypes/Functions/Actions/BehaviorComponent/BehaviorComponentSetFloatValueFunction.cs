using P2XMLEditor.GameData.Enums;
using System.Collections.Generic;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions.Actions.BehaviorComponent;

[Function("BehaviorComponent.SetFloatValue")]
public class BehaviorComponentSetFloatValueFunction(
	VirtualMachine vm,
	string[] parameters)
	: VmFunction {
	public override VmType ReturnType => VmType.Void;
	public override int ParamCount => 2;
	public FunctionSourceParam<string> VariableName { get; } = FunctionSourceParam<string>.Read(parameters[0], vm);
	public FunctionSourceParam<float> Value { get; } = FunctionSourceParam<float>.Read(parameters[1], vm);
	public override List<string>? GetParamStrings() => [VariableName.Write(), Value.Write()];
}