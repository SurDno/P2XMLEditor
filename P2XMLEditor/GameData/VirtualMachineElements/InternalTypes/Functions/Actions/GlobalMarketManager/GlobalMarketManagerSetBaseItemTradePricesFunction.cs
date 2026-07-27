using System.Collections.Generic;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions.Actions.GlobalMarketManager;

[Function("GlobalMarketManager.SetBaseItemTradePrices")]
public class GlobalMarketManagerSetBaseItemTradePricesFunction : VmFunction {
	public override FunctionReturnType ReturnType => FunctionReturnType.Void;
	public override int ParamCount => 3;
	public FunctionSourceParam<string>? ItemEntity { get; }
	public FunctionSourceParam<string>? BuyPrice { get; }
	public FunctionSourceParam<string>? SellPrice { get; }
	public override List<string>? GetParamStrings() => [ItemEntity?.Write() ?? "", BuyPrice?.Write() ?? "", SellPrice?.Write() ?? ""];
	public GlobalMarketManagerSetBaseItemTradePricesFunction(VirtualMachine vm, string[] parameters) {
		ItemEntity = FunctionSourceParam<string>.Read(parameters[0], vm);
		BuyPrice = FunctionSourceParam<string>.Read(parameters[1], vm);
		SellPrice = FunctionSourceParam<string>.Read(parameters[2], vm);
	}
}
