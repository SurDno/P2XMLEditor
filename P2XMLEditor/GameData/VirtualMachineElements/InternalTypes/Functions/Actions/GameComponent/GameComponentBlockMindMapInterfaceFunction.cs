using System.Collections.Generic;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions.Actions.GameComponent;

[Function("GameComponent.BlockMindMapInterface")]
public class GameComponentBlockMindMapInterfaceFunction(
	VirtualMachine vm,
	string[] parameters)
	: VmFunction {
	public override FunctionReturnType ReturnType => FunctionReturnType.Void;
	public override int ParamCount => 1;
	public FunctionSourceParam<bool> Block { get; } = FunctionSourceParam<bool>.Read(parameters[0], vm);
	public override List<string>? GetParamStrings() => [Block.Write()];
}
