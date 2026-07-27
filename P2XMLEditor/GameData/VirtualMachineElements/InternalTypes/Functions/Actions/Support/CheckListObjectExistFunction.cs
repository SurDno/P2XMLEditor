using System.Collections.Generic;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions.Actions.Support;

[Function("Support.CheckListObjectExist")]
public class CheckListObjectExistFunction : VmFunction {
	public override FunctionReturnType ReturnType => FunctionReturnType.Bool;
	public override int ParamCount => 2;
	public FunctionSourceParam<CommonList>? List { get; }
	public FunctionSourceParam<GameObject>? Target { get; }
	public override List<string>? GetParamStrings() => [List?.Write() ?? "", Target?.Write() ?? ""];
	public CheckListObjectExistFunction(VirtualMachine vm, string[] parameters) {
		List = FunctionSourceParam<CommonList>.Read(parameters[0], vm);
		Target = FunctionSourceParam<GameObject>.Read(parameters[1], vm);
	}
}
