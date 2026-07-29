using P2XMLEditor.GameData.Enums;
using System.Collections.Generic;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions.Actions.GlobalMarketManager;

[Function("GlobalMarketManager.SetBaseItemTradeBuyPrice")]
public class GlobalMarketManagerSetBaseItemTradeBuyPriceFunction(
	VirtualMachine vm,
	string[] parameters)
	: VmFunction {
	public override VmType ReturnType => VmType.Void;
	public override int ParamCount => 3;
	public FunctionSourceParam<BlueprintRef> Template { get; } = FunctionSourceParam<BlueprintRef>.Read((parameters.Length != 0) ? parameters[0] : "", vm);
	public FunctionSourceParam<float> Price { get; } = FunctionSourceParam<float>.Read((parameters.Length > 1) ? parameters[1] : "", vm);
	public FunctionSourceParam<float> BuyPrice { get; } = FunctionSourceParam<float>.Read((parameters.Length > 2) ? parameters[2] : "", vm);
	public override List<string>? GetParamStrings() => [Template.Write(), Price.Write(), BuyPrice.Write()];
}