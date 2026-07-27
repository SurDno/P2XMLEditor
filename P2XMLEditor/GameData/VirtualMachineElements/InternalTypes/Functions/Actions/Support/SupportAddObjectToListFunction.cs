using System.Collections.Generic;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions.Actions.Support;

[Function("Support.AddObjectToList")]
public class SupportAddObjectToListFunction(VirtualMachine vm, string[] parameters)
	: VmFunction {
	public override FunctionReturnType ReturnType => FunctionReturnType.Void;
	public override int ParamCount => 2;
	public FunctionSourceParam<CommonList>? ListParam { get; } = FunctionSourceParam<CommonList>.Read(parameters[0], vm);
	public FunctionSourceParam<GameObject>? ObjectParam { get; } = FunctionSourceParam<GameObject>.Read(parameters[1], vm);
	public override List<string>? GetParamStrings() => [ListParam.Write(), ObjectParam.Write()];
}
