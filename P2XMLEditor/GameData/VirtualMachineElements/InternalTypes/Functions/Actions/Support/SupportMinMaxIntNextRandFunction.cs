using P2XMLEditor.GameData.Enums;
using System.Collections.Generic;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions.Actions.Support;

[Function("Support.MinMaxIntNextRand")]
public class SupportMinMaxIntNextRandFunction(VirtualMachine vm, string[] parameters)
	: VmFunction {
	public override VmType ReturnType => VmType.Int32;
	public override int ParamCount => 3;
	public FunctionSourceParam<int> MinValue { get; } = FunctionSourceParam<int>.Read((parameters.Length != 0) ? parameters[0] : "", vm);
	public FunctionSourceParam<int> MaxValue { get; } = FunctionSourceParam<int>.Read((parameters.Length > 1) ? parameters[1] : "", vm);
	public FunctionSourceParam<int> PrevValue { get; } = FunctionSourceParam<int>.Read((parameters.Length > 2) ? parameters[2] : "", vm);
	public override List<string>? GetParamStrings() => [MinValue.Write(), MaxValue.Write(), PrevValue.Write()];
}