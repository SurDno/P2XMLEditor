using P2XMLEditor.GameData.Enums;
using System.Collections.Generic;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions.Actions.GameComponent;

[Function("GameComponent.IsObjectDerivedFromTemplate")]
public class GameComponentIsObjectDerivedFromTemplateFunction(
	VirtualMachine vm,
	string[] parameters)
	: VmFunction {
	public override VmType ReturnType => VmType.Boolean;
	public override int ParamCount => 2;
	public FunctionSourceParam<GameObject> ObjRef { get; } = FunctionSourceParam<GameObject>.Read(parameters[0], vm);
	public FunctionSourceParam<BlueprintRef> ClassRef { get; } = FunctionSourceParam<BlueprintRef>.Read(parameters[1], vm);
	public override List<string>? GetParamStrings() => [ObjRef.Write(), ClassRef.Write()];
}