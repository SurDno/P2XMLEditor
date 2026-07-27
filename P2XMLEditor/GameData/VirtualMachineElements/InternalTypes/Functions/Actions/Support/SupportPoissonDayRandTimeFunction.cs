using System.Collections.Generic;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions.Actions.Support;

[Function("Support.PoissonDayRandTime")]
public class SupportPoissonDayRandTimeFunction(VirtualMachine vm, string[] parameters)
	: VmFunction {
	public override FunctionReturnType ReturnType => FunctionReturnType.GameTime;
	public override int ParamCount => 1;
	public FunctionSourceParam<float>? Lambda { get; } = FunctionSourceParam<float>.Read(parameters[0], vm);
	public override List<string>? GetParamStrings() => [Lambda.Write()];
}
