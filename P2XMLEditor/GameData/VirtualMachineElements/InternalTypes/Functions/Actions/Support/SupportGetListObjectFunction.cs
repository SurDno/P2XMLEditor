using P2XMLEditor.GameData.Enums;
using System.Collections.Generic;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions.Actions.Support;

[Function("Support.GetListObject")]
public class SupportGetListObjectFunction(VirtualMachine vm, string[] parameters)
	: VmFunction {
	public override VmType ReturnType => VmType.GameObject;
	public override int ParamCount => 2;
	public FunctionSourceParam<CommonList>? List { get; } = FunctionSourceParam<CommonList>.Read(parameters[0], vm);
	public FunctionSourceParam<int>? Index { get; } = FunctionSourceParam<int>.Read(parameters[1], vm);
	public override List<string>? GetParamStrings() => [List.Write(), Index.Write()];
}