using P2XMLEditor.GameData.Enums;
using System.Collections.Generic;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions.Actions.GameComponent;

[Function("GameComponent.CreateObjectTo")]
public class GameComponentCreateObjectToFunction(VirtualMachine vm, string[] parameters)
	: VmFunction {
	public override VmType ReturnType => VmType.GameObject;
	public override int ParamCount => 2;
	public FunctionSourceParam<BlueprintRef> Template { get; } = FunctionSourceParam<BlueprintRef>.Read(parameters[0], vm);
	public FunctionSourceParam<ObjRef> Destination { get; } = FunctionSourceParam<ObjRef>.Read(parameters[1], vm);
	public override List<string>? GetParamStrings() => [Template.Write(), Destination.Write()];
}