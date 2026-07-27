using System.Collections.Generic;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions.Actions.GameComponent;

[Function("GameComponent.GetNearestOwnerByFunctional")]
public class GameComponentGetNearestOwnerByFunctionalFunction(
	VirtualMachine vm,
	string[] parameters)
	: VmFunction {
	public override FunctionReturnType ReturnType => FunctionReturnType.ObjRef;
	public override int ParamCount => 2;
	public CommonVariable? Obj { get; } = CommonVariable.Read(parameters[0], vm);
	public FunctionSourceParam<string>? FunctionalName { get; } = FunctionSourceParam<string>.Read(parameters[1], vm);
	public override List<string>? GetParamStrings() => [Obj.Write(), FunctionalName?.Write() ?? ""];
}
