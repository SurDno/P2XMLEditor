using System.Collections.Generic;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions.Actions.Storage;

[Function("Storage.CheckExistCombinationElement")]
public class StorageCheckExistCombinationElementFunction(
	VirtualMachine vm,
	string[] parameters)
	: VmFunction {
	public override FunctionReturnType ReturnType => FunctionReturnType.Bool;
	public override int ParamCount => 1;
	public FunctionSourceParam<BlueprintRef> CombinationElement { get; } = FunctionSourceParam<BlueprintRef>.Read(parameters[0], vm);
	public override List<string>? GetParamStrings() => [CombinationElement.Write()];
}
