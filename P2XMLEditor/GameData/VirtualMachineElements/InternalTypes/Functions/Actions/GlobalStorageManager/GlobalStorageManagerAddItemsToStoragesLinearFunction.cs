using P2XMLEditor.GameData.Enums;
using System.Collections.Generic;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions.Actions.GlobalStorageManager;

[Function("GlobalStorageManager.AddItemsToStoragesLinear")]
public class GlobalStorageManagerAddItemsToStoragesLinearFunction : VmFunction {
	public override VmType ReturnType => VmType.Void;
	public override int ParamCount => 6;
	public FunctionSourceParam<string>? StoragesRootInfo { get; }
	public FunctionSourceParam<string>? StorageTags { get; }
	public FunctionSourceParam<string>? ContainerTypes { get; }
	public FunctionSourceParam<string>? ContainerTags { get; }
	public FunctionSourceParam<string>? ContainerStatusesData { get; }
	public FunctionSourceParam<string>? StorageTypes { get; }
	public override List<string>? GetParamStrings() => [StoragesRootInfo?.Write() ?? "", StorageTags?.Write() ?? "", ContainerTypes?.Write() ?? "", ContainerTags?.Write() ?? "", ContainerStatusesData?.Write() ?? "", StorageTypes?.Write() ?? ""];
	public GlobalStorageManagerAddItemsToStoragesLinearFunction(VirtualMachine vm, string[] parameters) {
		StoragesRootInfo = FunctionSourceParam<string>.Read(parameters[0], vm);
		StorageTags = FunctionSourceParam<string>.Read(parameters[1], vm);
		ContainerTypes = FunctionSourceParam<string>.Read(parameters[2], vm);
		ContainerTags = FunctionSourceParam<string>.Read(parameters[3], vm);
		ContainerStatusesData = FunctionSourceParam<string>.Read(parameters[4], vm);
		StorageTypes = FunctionSourceParam<string>.Read(parameters[5], vm);
	}
}