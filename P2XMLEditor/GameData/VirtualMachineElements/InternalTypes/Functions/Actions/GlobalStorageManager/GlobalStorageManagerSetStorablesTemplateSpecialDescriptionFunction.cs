using System.Collections.Generic;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions.Actions.GlobalStorageManager;

[Function("GlobalStorageManager.SetStorablesTemplateSpecialDescription")]
public class GlobalStorageManagerSetStorablesTemplateSpecialDescriptionFunction(
	VirtualMachine vm,
	string[] parameters)
	: VmFunction {
	public override FunctionReturnType ReturnType => FunctionReturnType.Void;
	public override int ParamCount => 2;
	public FunctionSourceParam<BlueprintRef> Template { get; } = FunctionSourceParam<BlueprintRef>.Read(parameters[0], vm);
	public FunctionSourceParam<GameString> Value { get; } = FunctionSourceParam<GameString>.Read(parameters[1], vm);
	public override List<string>? GetParamStrings() => [Template.Write(), Value.Write()];
}
