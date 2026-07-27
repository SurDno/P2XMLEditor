using System.Collections.Generic;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions.Actions.GlobalStorageManager;

[Function("GlobalStorageManager.SetStorageContainerParams")]
public class GlobalStorageManagerSetStorageContainerParamsFunction : VmFunction {
	public override FunctionReturnType ReturnType => FunctionReturnType.Void;
	public override int ParamCount => 6;
	public FunctionSourceParam<string>? StorageGroup { get; }
	public FunctionSourceParam<string>? MinItems { get; }
	public FunctionSourceParam<string>? MaxItems { get; }
	public FunctionSourceParam<string>? Title { get; }
	public FunctionSourceParam<string>? OpenAction { get; }
	public FunctionSourceParam<string>? FurnitureTemplates { get; }
	public override List<string>? GetParamStrings() => [StorageGroup?.Write() ?? "", MinItems?.Write() ?? "", MaxItems?.Write() ?? "", Title?.Write() ?? "", OpenAction?.Write() ?? "", FurnitureTemplates?.Write() ?? ""];
	public GlobalStorageManagerSetStorageContainerParamsFunction(VirtualMachine vm, string[] parameters) {
		StorageGroup = FunctionSourceParam<string>.Read(parameters[0], vm);
		MinItems = FunctionSourceParam<string>.Read(parameters[1], vm);
		MaxItems = FunctionSourceParam<string>.Read(parameters[2], vm);
		Title = FunctionSourceParam<string>.Read(parameters[3], vm);
		OpenAction = FunctionSourceParam<string>.Read(parameters[4], vm);
		FurnitureTemplates = FunctionSourceParam<string>.Read(parameters[5], vm);
	}
}
