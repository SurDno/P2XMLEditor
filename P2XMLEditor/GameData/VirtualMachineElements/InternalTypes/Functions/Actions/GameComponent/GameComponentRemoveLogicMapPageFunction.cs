using System.Collections.Generic;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions.Actions.GameComponent;

[Function("GameComponent.RemoveLogicMapPage")]
public class GameComponentRemoveLogicMapPageFunction(
	VirtualMachine vm,
	string[] parameters)
	: VmFunction {
	public override FunctionReturnType ReturnType => FunctionReturnType.Void;
	public override int ParamCount => 1;
	public FunctionSourceParam<MindMap>? RemMMPage { get; } = FunctionSourceParam<MindMap>.Read(parameters[0], vm);
	public override List<string>? GetParamStrings() => [RemMMPage.Write()];
}
