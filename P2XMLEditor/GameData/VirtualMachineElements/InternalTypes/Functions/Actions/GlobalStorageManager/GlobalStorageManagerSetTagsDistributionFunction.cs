using System.Collections.Generic;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions.Actions.GlobalStorageManager;

[Function("GlobalStorageManager.SetTagsDistribution")]
public class GlobalStorageManagerSetTagsDistributionFunction(
	VirtualMachine vm,
	string[] parameters)
	: VmFunction {
	public override FunctionReturnType ReturnType => FunctionReturnType.Void;
	public override int ParamCount => 3;
	public FunctionSourceParam<string> StoragesRootInfo { get; } = FunctionSourceParam<string>.Read((parameters.Length != 0) ? parameters[0] : "", vm);
	public FunctionSourceParam<string> StoragTagDistributionInfo { get; } = FunctionSourceParam<string>.Read((parameters.Length > 1) ? parameters[1] : "", vm);
	public FunctionSourceParam<string> StorageTypes { get; } = FunctionSourceParam<string>.Read((parameters.Length > 2) ? parameters[2] : "", vm);
	public override List<string>? GetParamStrings() => [StoragesRootInfo.Write(), StoragTagDistributionInfo.Write(), StorageTypes.Write()];
}
