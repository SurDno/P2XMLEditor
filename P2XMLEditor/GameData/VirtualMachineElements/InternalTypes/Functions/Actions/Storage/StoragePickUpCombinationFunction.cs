using System.Collections.Generic;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions.Actions.Storage;

[Function("Storage.PickUpCombination")]
public class StoragePickUpCombinationFunction(VirtualMachine vm, string[] parameters)
	: VmFunction {
	public override FunctionReturnType ReturnType => FunctionReturnType.Void;
	public override int ParamCount => 1;
	public FunctionSourceParam<BlueprintRef> CombinationObject { get; } = FunctionSourceParam<BlueprintRef>.Read(parameters[0], vm);
	public override List<string>? GetParamStrings() => [CombinationObject.Write()];
}
