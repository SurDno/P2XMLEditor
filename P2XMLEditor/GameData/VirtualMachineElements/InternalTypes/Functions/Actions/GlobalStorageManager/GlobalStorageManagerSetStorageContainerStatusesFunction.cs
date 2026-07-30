using P2XMLEditor.GameData.Enums;
using System.Collections.Generic;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions.Actions.GlobalStorageManager;

[Function("GlobalStorageManager.SetStorageContainerStatuses")]
public class GlobalStorageManagerSetStorageContainerStatusesFunction(
	VirtualMachine vm,
	string[] parameters)
	: VmFunction {
	public override VmType ReturnType => VmType.Void;
	public override int ParamCount => 6;
	public FunctionSourceParam<string> StoragesRootInfo { get; } = FunctionSourceParam<string>.Read(parameters[0], vm);
	public FunctionSourceParam<string> StorageTags { get; } = FunctionSourceParam<string>.Read(parameters[1], vm);
	public FunctionSourceParam<string> ContainerTypes { get; } = FunctionSourceParam<string>.Read(parameters[2], vm);
	public FunctionSourceParam<string> ContainerTags { get; } = FunctionSourceParam<string>.Read(parameters[3], vm);
	public FunctionSourceParam<string> ContainerStatusesData { get; } = FunctionSourceParam<string>.Read(parameters[4], vm);
	public FunctionSourceParam<string> StorageTypes { get; } = FunctionSourceParam<string>.Read(parameters[5], vm);
	public override List<string>? GetParamStrings() => [StoragesRootInfo.Write(), StorageTags.Write(), ContainerTypes.Write(), ContainerTags.Write(), ContainerStatusesData.Write(), StorageTypes.Write()];
}