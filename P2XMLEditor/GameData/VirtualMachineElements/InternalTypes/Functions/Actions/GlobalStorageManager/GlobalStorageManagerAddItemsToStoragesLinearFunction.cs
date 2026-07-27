using System.Collections.Generic;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions.Actions.GlobalStorageManager;

[Function("GlobalStorageManager.AddItemsToStoragesLinear")]
public class GlobalStorageManagerAddItemsToStoragesLinearFunction : VmFunction {
	public override FunctionReturnType ReturnType => FunctionReturnType.Void;
	public override int ParamCount => 6;
	public FunctionSourceParam<string>? StorageGroup { get; }
	public FunctionSourceParam<string>? Probability { get; }
	public FunctionSourceParam<string>? ItemList { get; }
	public FunctionSourceParam<string>? WeightList { get; }
	public FunctionSourceParam<string>? MinCountList { get; }
	public FunctionSourceParam<string>? MaxCountList { get; }
	public override List<string>? GetParamStrings() => [StorageGroup?.Write() ?? "", Probability?.Write() ?? "", ItemList?.Write() ?? "", WeightList?.Write() ?? "", MinCountList?.Write() ?? "", MaxCountList?.Write() ?? ""];
	public GlobalStorageManagerAddItemsToStoragesLinearFunction(VirtualMachine vm, string[] parameters) {
		StorageGroup = FunctionSourceParam<string>.Read(parameters[0], vm);
		Probability = FunctionSourceParam<string>.Read(parameters[1], vm);
		ItemList = FunctionSourceParam<string>.Read(parameters[2], vm);
		WeightList = FunctionSourceParam<string>.Read(parameters[3], vm);
		MinCountList = FunctionSourceParam<string>.Read(parameters[4], vm);
		MaxCountList = FunctionSourceParam<string>.Read(parameters[5], vm);
	}
}
