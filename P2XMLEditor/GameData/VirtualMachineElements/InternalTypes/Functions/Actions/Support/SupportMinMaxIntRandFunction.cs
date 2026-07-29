using P2XMLEditor.GameData.Enums;
using System.Collections.Generic;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions.Actions.Support;

[Function("Support.MinMaxIntRand")]
public class SupportMinMaxIntRandFunction(VirtualMachine vm, string[] parameters)
	: VmFunction {
	public override VmType ReturnType => VmType.Int32;
	public override int ParamCount => 2;
	public FunctionSourceParam<int> MinParam { get; } = FunctionSourceParam<int>.Read(parameters[0], vm);
	public FunctionSourceParam<int> MaxParam { get; } = FunctionSourceParam<int>.Read(parameters[1], vm);
	public override List<string>? GetParamStrings() => [MinParam.Write(), MaxParam.Write()];
}