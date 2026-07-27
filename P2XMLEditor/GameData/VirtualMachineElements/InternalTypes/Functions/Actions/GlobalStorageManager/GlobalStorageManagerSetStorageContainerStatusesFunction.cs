using System.Collections.Generic;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions.Actions.GlobalStorageManager;

[Function("GlobalStorageManager.SetStorageContainerStatuses")]
public class GlobalStorageManagerSetStorageContainerStatusesFunction(
	VirtualMachine vm,
	string[] parameters)
	: VmFunction {
	public override FunctionReturnType ReturnType => FunctionReturnType.Void;
	public override int ParamCount => 6;
	public FunctionSourceParam<string> StoragesRootInfo { get; } = FunctionSourceParam<string>.Read((parameters.Length != 0) ? parameters[0] : "", vm);
	public FunctionSourceParam<string> StorageTags { get; } = FunctionSourceParam<string>.Read((parameters.Length > 1) ? parameters[1] : "", vm);
	public FunctionSourceParam<string> ContainerTypes { get; } = FunctionSourceParam<string>.Read((parameters.Length > 2) ? parameters[2] : "", vm);
	public FunctionSourceParam<string> ContainerTags { get; } = FunctionSourceParam<string>.Read((parameters.Length > 3) ? parameters[3] : "", vm);
	public FunctionSourceParam<string> ContainerStatusesData { get; } = FunctionSourceParam<string>.Read((parameters.Length > 4) ? parameters[4] : "", vm);
	public FunctionSourceParam<string> StorageTypes { get; } = FunctionSourceParam<string>.Read((parameters.Length > 5) ? parameters[5] : "", vm);
	public override List<string>? GetParamStrings() => [StoragesRootInfo.Write(), StorageTags.Write(), ContainerTypes.Write(), ContainerTags.Write(), ContainerStatusesData.Write(), StorageTypes.Write()];
}
