using System.Collections.Generic;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions.Actions.Support;

[Function("Support.MergeLists")]
public class SupportMergeListsFunction(VirtualMachine vm, string[] parameters)
	: VmFunction {
	public override FunctionReturnType ReturnType => FunctionReturnType.Void;
	public override int ParamCount => 2;
	public FunctionSourceParam<CommonList>? ListA { get; } = FunctionSourceParam<CommonList>.Read(parameters[0], vm);
	public FunctionSourceParam<CommonList>? ListB { get; } = FunctionSourceParam<CommonList>.Read(parameters[1], vm);
	public override List<string>? GetParamStrings() => [ListA.Write(), ListB.Write()];
}
