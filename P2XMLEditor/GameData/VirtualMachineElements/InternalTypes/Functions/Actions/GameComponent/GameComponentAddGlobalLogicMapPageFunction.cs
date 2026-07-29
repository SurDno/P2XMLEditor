using P2XMLEditor.GameData.Enums;
using System.Collections.Generic;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions.Actions.GameComponent;

[Function("GameComponent.AddGlobalLogicMapPage")]
public class GameComponentAddGlobalLogicMapPageFunction(
	VirtualMachine vm,
	string[] parameters)
	: VmFunction {
	public override VmType ReturnType => VmType.Void;
	public override int ParamCount => 1;
	public FunctionSourceParam<MindMap>? MMPage { get; } = FunctionSourceParam<MindMap>.Read(parameters[0], vm);
	public override List<string>? GetParamStrings() => [MMPage.Write()];
}