using System.Collections.Generic;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions.Actions.GlobalMarketManager;

[Function("GlobalMarketManager.SetBaseItemTradePriceFactors")]
public class GlobalMarketManagerSetBaseItemTradePriceFactorsFunction : VmFunction {
	public override FunctionReturnType ReturnType => FunctionReturnType.Void;
	public override int ParamCount => 3;
	public FunctionSourceParam<string> ItemEntity { get; }
	public FunctionSourceParam<string> BuyFactor { get; }
	public FunctionSourceParam<string> SellFactor { get; }
	public override List<string>? GetParamStrings() => [ItemEntity.Write(), BuyFactor.Write(), SellFactor.Write()];
	public GlobalMarketManagerSetBaseItemTradePriceFactorsFunction(VirtualMachine vm, string[] parameters) {
		ItemEntity = FunctionSourceParam<string>.Read(parameters[0], vm);
		BuyFactor = FunctionSourceParam<string>.Read(parameters[1], vm);
		SellFactor = FunctionSourceParam<string>.Read(parameters[2], vm);
	}
}
