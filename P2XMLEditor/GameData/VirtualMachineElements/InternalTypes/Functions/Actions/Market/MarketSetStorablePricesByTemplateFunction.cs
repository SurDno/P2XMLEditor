using P2XMLEditor.GameData.Enums;
using System.Collections.Generic;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions.Actions.Market;

[Function("Market.SetStorablePricesByTemplate")]
public class MarketSetStorablePricesByTemplateFunction(
	VirtualMachine vm,
	string[] parameters)
	: VmFunction {
	public override VmType ReturnType => VmType.Void;
	public override int ParamCount => 3;
	public FunctionSourceParam<EntityRef> Template { get; } = FunctionSourceParam<EntityRef>.Read(parameters[0], vm);
	public FunctionSourceParam<float> PriceValue { get; } = FunctionSourceParam<float>.Read(parameters[1], vm);
	public FunctionSourceParam<float> BuyPriceValue { get; } = FunctionSourceParam<float>.Read(parameters[2], vm);
	public override List<string>? GetParamStrings() => [Template.Write(), PriceValue.Write(), BuyPriceValue.Write()];
}