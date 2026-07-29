using P2XMLEditor.GameData.Enums;
using System.Collections.Generic;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions.Actions.GameComponent;

[Function("GameComponent.GetObjectWithTag")]
public class GameComponentGetObjectWithTagFunction(
	VirtualMachine vm,
	string[] parameters)
	: VmFunction {
	public override VmType ReturnType => VmType.GameObject;
	public override int ParamCount => 2;
	public FunctionSourceParam<GameObject> Object { get; } = FunctionSourceParam<GameObject>.Read(parameters[0], vm);
	public FunctionSourceParam<string> Tag { get; } = FunctionSourceParam<string>.Read(parameters[1], vm);
	public override List<string>? GetParamStrings() => [Object.Write(), Tag.Write()];
}