using P2XMLEditor.GameData.Enums;
using System.Collections.Generic;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions.Actions.Support;

[Function("Support.PoissonHourRandTime")]
public class SupportPoissonHourRandTimeFunction(VirtualMachine vm, string[] parameters)
	: VmFunction {
	public override VmType ReturnType => VmType.GameTime;
	public override int ParamCount => 1;
	public FunctionSourceParam<float> FlowPerHour { get; } = FunctionSourceParam<float>.Read(parameters[0], vm);
	public override List<string>? GetParamStrings() => [FlowPerHour.Write()];
}