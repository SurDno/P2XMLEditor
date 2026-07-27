using System.Collections.Generic;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions.Actions.GameComponent;

[Function("GameComponent.StartBlueprint")]
public class GameComponentStartBlueprintFunction(VirtualMachine vm, string[] parameters)
	: VmFunction {
	public override FunctionReturnType ReturnType => FunctionReturnType.Void;
	public override int ParamCount => 2;
	public FunctionSourceParam<BlueprintRef> ScriptRef { get; } = FunctionSourceParam<BlueprintRef>.Read(parameters[0], vm);
	public FunctionSourceParam<EntityRef> Target { get; } = FunctionSourceParam<EntityRef>.Read(parameters[1], vm);
	public override List<string>? GetParamStrings() => [ScriptRef.Write(), Target.Write()];
}
