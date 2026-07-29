using P2XMLEditor.GameData.Enums;
using System.Collections.Generic;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions.Actions.Market;

[Function("Market.SetMultiStorablesFixedPrices")]
public class MarketSetMultiStorablesFixedPricesFunction : VmFunction {
	public override VmType ReturnType => VmType.Void;
	public override int ParamCount => 3;
	public FunctionSourceParam<string>? StorageGroup { get; }
	public FunctionSourceParam<string>? BuyPrice { get; }
	public FunctionSourceParam<string>? SellPrice { get; }
	public override List<string>? GetParamStrings() => [StorageGroup?.Write() ?? "", BuyPrice?.Write() ?? "", SellPrice?.Write() ?? ""];
	public MarketSetMultiStorablesFixedPricesFunction(VirtualMachine vm, string[] parameters) {
		StorageGroup = FunctionSourceParam<string>.Read(parameters[0], vm);
		BuyPrice = FunctionSourceParam<string>.Read(parameters[1], vm);
		SellPrice = FunctionSourceParam<string>.Read(parameters[2], vm);
	}
}