using P2XMLEditor.GameData.Enums;
using System.Collections.Generic;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions.Actions.GlobalStorageManager;

[Function("GlobalStorageManager.SetTagsDistribution")]
public class GlobalStorageManagerSetTagsDistributionFunction(
	VirtualMachine vm,
	string[] parameters)
	: VmFunction {
	public override VmType ReturnType => VmType.Void;
	public override int ParamCount => 3;
	public FunctionSourceParam<string> StoragesRootInfo { get; } = FunctionSourceParam<string>.Read(parameters[0], vm);
	public FunctionSourceParam<string> StoragTagDistributionInfo { get; } = FunctionSourceParam<string>.Read(parameters[1], vm);
	public FunctionSourceParam<string> StorageTypes { get; } = FunctionSourceParam<string>.Read(parameters[2], vm);
	public override List<string>? GetParamStrings() => [StoragesRootInfo.Write(), StoragTagDistributionInfo.Write(), StorageTypes.Write()];
}