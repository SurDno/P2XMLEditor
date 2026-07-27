using System.Collections.Generic;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions.Actions.Market;

[Function("Market.SetStorablePricesByTemplate")]
public class MarketSetStorablePricesByTemplateFunction(
	VirtualMachine vm,
	string[] parameters)
	: VmFunction {
	public override FunctionReturnType ReturnType => FunctionReturnType.Void;
	public override int ParamCount => 3;
	public FunctionSourceParam<EntityRef> Template { get; } = FunctionSourceParam<EntityRef>.Read((parameters.Length != 0) ? parameters[0] : "", vm);
	public FunctionSourceParam<float> PriceValue { get; } = FunctionSourceParam<float>.Read((parameters.Length > 1) ? parameters[1] : "", vm);
	public FunctionSourceParam<float> BuyPriceValue { get; } = FunctionSourceParam<float>.Read((parameters.Length > 2) ? parameters[2] : "", vm);
	public override List<string>? GetParamStrings() => [Template.Write(), PriceValue.Write(), BuyPriceValue.Write()];
}
