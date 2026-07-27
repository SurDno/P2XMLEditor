using System.Collections.Generic;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions.Actions.Storage;

[Function("Storage.PickUpCombinationWithDrop")]
public class StoragePickUpCombinationWithDropFunction(
	VirtualMachine vm,
	string[] parameters)
	: VmFunction {
	public override FunctionReturnType ReturnType => FunctionReturnType.Void;
	public override int ParamCount => 2;
	public FunctionSourceParam<BlueprintRef> CombinationObject { get; } = FunctionSourceParam<BlueprintRef>.Read(parameters[0], vm);
	public FunctionSourceParam<bool> DropExisting { get; } = FunctionSourceParam<bool>.Read(parameters[1], vm);
	public override List<string>? GetParamStrings() => [CombinationObject.Write(), DropExisting.Write()];
}
