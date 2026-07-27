using System.Collections.Generic;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions.Actions.GameComponent;

[Function("GameComponent.ProcessCustomGroupObjectAction")]
public class GameComponentProcessCustomGroupObjectActionFunction(
	VirtualMachine vm,
	string[] parameters)
	: VmFunction {
	public override FunctionReturnType ReturnType => FunctionReturnType.Void;
	public override int ParamCount => 2;
	public FunctionSourceParam<string>? StoragesRootInfo { get; } = FunctionSourceParam<string>.Read(parameters[0], vm);
	public FunctionSourceParam<string>? ActionInfo { get; } = FunctionSourceParam<string>.Read(parameters[1], vm);
	public override List<string>? GetParamStrings() => [StoragesRootInfo?.Write() ?? "", ActionInfo?.Write() ?? ""];
}
