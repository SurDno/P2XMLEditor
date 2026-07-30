using P2XMLEditor.GameData.Enums;
using System.Collections.Generic;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions.Actions.GlobalMarketManager;

[Function("GlobalMarketManager.SetBaseItemTradePrice")]
public class GlobalMarketManagerSetBaseItemTradePriceFunction(
	VirtualMachine vm,
	string[] parameters)
	: VmFunction {
	public override VmType ReturnType => VmType.Void;
	public override int ParamCount => 2;
	public FunctionSourceParam<BlueprintRef> Template { get; } = FunctionSourceParam<BlueprintRef>.Read(parameters[0], vm);
	public FunctionSourceParam<float> Price { get; } = FunctionSourceParam<float>.Read(parameters[1], vm);
	public override List<string>? GetParamStrings() => [Template.Write(), Price.Write()];
}