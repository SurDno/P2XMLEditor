using P2XMLEditor.GameData.Enums;
using System.Collections.Generic;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions.Actions.DiseaseComponent;

[Function("DiseaseComponent.SetDiseaseValue")]
public class DiseaseComponentSetDiseaseValueFunction(
	VirtualMachine vm,
	string[] parameters)
	: VmFunction {
	public override VmType ReturnType => VmType.Void;
	public override int ParamCount => 2;
	public FunctionSourceParam<float> Value { get; } = FunctionSourceParam<float>.Read(parameters[0], vm);
	public FunctionSourceParam<GameTime> Delta { get; } = FunctionSourceParam<GameTime>.Read(parameters[1], vm);
	public override List<string>? GetParamStrings() => [Value.Write(), Delta.Write()];
}