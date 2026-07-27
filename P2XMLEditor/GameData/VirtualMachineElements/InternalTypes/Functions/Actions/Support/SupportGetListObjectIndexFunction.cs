using System.Collections.Generic;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions.Actions.Support;

[Function("Support.GetListObjectIndex")]
public class SupportGetListObjectIndexFunction(VirtualMachine vm, string[] parameters)
	: VmFunction {
	public override FunctionReturnType ReturnType => FunctionReturnType.Int;
	public override int ParamCount => 2;
	public FunctionSourceParam<CommonList>? List { get; } = FunctionSourceParam<CommonList>.Read(parameters[0], vm);
	public FunctionSourceParam<GameObject>? Entity { get; } = FunctionSourceParam<GameObject>.Read(parameters[1], vm);
	public override List<string>? GetParamStrings() => [List.Write(), Entity.Write()];
}
