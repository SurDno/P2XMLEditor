using System.Collections.Generic;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions.Actions.GlobalMarketManager;

[Function("GlobalMarketManager.SetItemStackCount")]
public class GlobalMarketManagerSetItemStackCountFunction(
	VirtualMachine vm,
	string[] parameters)
	: VmFunction {
	public override FunctionReturnType ReturnType => FunctionReturnType.Void;
	public override int ParamCount => 2;
	public FunctionSourceParam<string> ItemTemplatesNames { get; } = FunctionSourceParam<string>.Read((parameters.Length != 0) ? parameters[0] : "", vm);
	public FunctionSourceParam<string> ItemTemplatesStackCountValues { get; } = FunctionSourceParam<string>.Read((parameters.Length > 1) ? parameters[1] : "", vm);
	public override List<string>? GetParamStrings() => [ItemTemplatesNames.Write(), ItemTemplatesStackCountValues.Write()];
}
