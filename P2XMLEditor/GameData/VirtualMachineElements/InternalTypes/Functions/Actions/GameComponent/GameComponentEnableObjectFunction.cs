using System.Collections.Generic;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions.Actions.GameComponent;

[Function("GameComponent.EnableObject")]
public class GameComponentEnableObjectFunction(VirtualMachine vm, string[] parameters)
	: VmFunction {
	public override FunctionReturnType ReturnType => FunctionReturnType.Void;
	public override int ParamCount => 2;
	public FunctionSourceParam<GameObject> Obj { get; } = FunctionSourceParam<GameObject>.Read(parameters[0], vm);
	public FunctionSourceParam<bool> Enable { get; } = FunctionSourceParam<bool>.Read(parameters[1], vm);
	public override List<string>? GetParamStrings() => [Obj.Write(), Enable.Write()];
}
