using P2XMLEditor.GameData.Enums;
using System.Collections.Generic;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions.Actions.Support;

[Function("Support.Rand")]
public class SupportRandFunction(VirtualMachine vm, string[] parameters)
	: VmFunction {
	public override VmType ReturnType => VmType.Single;
	public override int ParamCount => 1;
	public FunctionSourceParam<float> MaxValue { get; } = FunctionSourceParam<float>.Read(parameters[0], vm);
	public override List<string>? GetParamStrings() => [MaxValue.Write()];
}