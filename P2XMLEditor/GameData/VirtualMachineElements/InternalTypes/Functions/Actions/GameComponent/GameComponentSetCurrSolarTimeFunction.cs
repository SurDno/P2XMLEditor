using System.Collections.Generic;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions.Actions.GameComponent;

[Function("GameComponent.SetCurrSolarTime")]
public class GameComponentSetCurrSolarTimeFunction(
	VirtualMachine vm,
	string[] parameters)
	: VmFunction {
	public override FunctionReturnType ReturnType => FunctionReturnType.Void;
	public override int ParamCount => 1;
	public FunctionSourceParam<GameTime>? Time { get; } = FunctionSourceParam<GameTime>.Read(parameters[0], vm);
	public override List<string>? GetParamStrings() => [Time.Write()];
}
