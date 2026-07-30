using P2XMLEditor.GameData.Enums;
using System.Collections.Generic;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions.Actions.GlobalStorageManager;

[Function("GlobalStorageManager.SetContainerTagsComplexDistribution")]
public class GlobalStorageManagerSetContainerTagsComplexDistributionFunction(
	VirtualMachine vm,
	string[] parameters)
	: VmFunction {
	public override VmType ReturnType => VmType.Void;
	public override int ParamCount => 5;
	public FunctionSourceParam<string> StoragesRootInfo { get; } = FunctionSourceParam<string>.Read(parameters[0], vm);
	public FunctionSourceParam<string> StorageTags { get; } = FunctionSourceParam<string>.Read(parameters[1], vm);
	public FunctionSourceParam<string> ContainerTypes { get; } = FunctionSourceParam<string>.Read(parameters[2], vm);
	public FunctionSourceParam<string> StoragTagDistributionInfo { get; } = FunctionSourceParam<string>.Read(parameters[3], vm);
	public FunctionSourceParam<string> StorageTypes { get; } = FunctionSourceParam<string>.Read(parameters[4], vm);
	public override List<string>? GetParamStrings() => [StoragesRootInfo.Write(), StorageTags.Write(), ContainerTypes.Write(), StoragTagDistributionInfo.Write(), StorageTypes.Write()];
}