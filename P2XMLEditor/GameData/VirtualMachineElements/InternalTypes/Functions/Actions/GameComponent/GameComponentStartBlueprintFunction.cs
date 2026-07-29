using System.Collections.Generic;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.Enums;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions.Actions.GameComponent;

[Function("GameComponent.StartBlueprint")]
public class GameComponentStartBlueprintFunction(VirtualMachine vm, string[] parameters)
	: VmFunction {
	public override VmType ReturnType => VmType.Void;
	public override int ParamCount => 2;	
	public FunctionSourceParam<Sample> ScriptRef { get; } =
                                        		FunctionSourceParam<Sample>.Read(parameters[0], vm, VmType.BlueprintObject);
	public FunctionSourceParam<EntityRef> Target { get; } = FunctionSourceParam<EntityRef>.Read(parameters[1], vm);
	public override List<string>? GetParamStrings() => [ScriptRef.Write(), Target.Write()];
}