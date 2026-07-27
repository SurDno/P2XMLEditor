using System.Collections.Generic;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions.Actions.GlobalMarketManager;

[Function("GlobalMarketManager.SetItemTradePriceFactor")]
public class GlobalMarketManagerSetItemTradePriceFactorFunction(
	VirtualMachine vm,
	string[] parameters)
	: VmFunction {
	public override FunctionReturnType ReturnType => FunctionReturnType.Void;
	public override int ParamCount => 2;
	public FunctionSourceParam<BlueprintRef> Template { get; } = FunctionSourceParam<BlueprintRef>.Read((parameters.Length != 0) ? parameters[0] : "", vm);
	public FunctionSourceParam<float> PriceFactor { get; } = FunctionSourceParam<float>.Read((parameters.Length > 1) ? parameters[1] : "", vm);
	public override List<string>? GetParamStrings() => [Template.Write(), PriceFactor.Write()];
}
