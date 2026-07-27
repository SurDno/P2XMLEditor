using System.Collections.Generic;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions.Actions.GameComponent;

[Function("GameComponent.SetCustomGroupObjectStatuses")]
public class GameComponentSetCustomGroupObjectStatusesFunction(
	VirtualMachine vm,
	string[] parameters)
	: VmFunction {
	public override FunctionReturnType ReturnType => FunctionReturnType.Void;
	public override int ParamCount => 3;
	public FunctionSourceParam<string>? StoragesRootInfo { get; } = FunctionSourceParam<string>.Read(parameters[0], vm);
	public FunctionSourceParam<string>? FuncComponentName { get; } = FunctionSourceParam<string>.Read(parameters[1], vm);
	public FunctionSourceParam<string>? ObjectStatusesData { get; } = FunctionSourceParam<string>.Read(parameters[2], vm);
	public override List<string>? GetParamStrings() => [StoragesRootInfo?.Write() ?? "", FuncComponentName?.Write() ?? "", ObjectStatusesData?.Write() ?? ""];
}
