using System.Collections.Generic;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions.Actions.Market;

[Function("Market.SetMultiStorablesPricesFactor")]
public class MarketSetMultiStorablesPricesFactorFunction : VmFunction {
	public override FunctionReturnType ReturnType => FunctionReturnType.Void;
	public override int ParamCount => 3;
	public FunctionSourceParam<string> StorageGroup { get; }
	public FunctionSourceParam<string> BuyFactor { get; }
	public FunctionSourceParam<string> SellFactor { get; }
	public override List<string>? GetParamStrings() => [StorageGroup.Write(), BuyFactor.Write(), SellFactor.Write()];
	public MarketSetMultiStorablesPricesFactorFunction(VirtualMachine vm, string[] parameters) {
		StorageGroup = FunctionSourceParam<string>.Read(parameters[0], vm);
		BuyFactor = FunctionSourceParam<string>.Read(parameters[1], vm);
		SellFactor = FunctionSourceParam<string>.Read(parameters[2], vm);
	}
}
