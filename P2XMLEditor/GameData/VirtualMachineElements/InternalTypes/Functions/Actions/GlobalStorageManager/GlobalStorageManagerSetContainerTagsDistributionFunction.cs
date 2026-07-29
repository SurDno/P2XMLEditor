using P2XMLEditor.GameData.Enums;
using System.Collections.Generic;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions.Actions.GlobalStorageManager;

[Function("GlobalStorageManager.SetContainerTagsDistribution")]
public class GlobalStorageManagerSetContainerTagsDistributionFunction : VmFunction {
	public override VmType ReturnType => VmType.Void;
	public override int ParamCount => 5;
	public FunctionSourceParam<string>? StorageGroup { get; }
	public FunctionSourceParam<string>? MinCount { get; }
	public FunctionSourceParam<string>? MaxCount { get; }
	public FunctionSourceParam<string>? Tags { get; }
	public FunctionSourceParam<string>? Weights { get; }
	public override List<string>? GetParamStrings() => [StorageGroup?.Write() ?? "", MinCount?.Write() ?? "", MaxCount?.Write() ?? "", Tags?.Write() ?? "", Weights?.Write() ?? ""];
	public GlobalStorageManagerSetContainerTagsDistributionFunction(VirtualMachine vm, string[] parameters) {
		StorageGroup = FunctionSourceParam<string>.Read(parameters[0], vm);
		MinCount = FunctionSourceParam<string>.Read(parameters[1], vm);
		MaxCount = FunctionSourceParam<string>.Read(parameters[2], vm);
		Tags = FunctionSourceParam<string>.Read(parameters[3], vm);
		Weights = FunctionSourceParam<string>.Read(parameters[4], vm);
	}
}