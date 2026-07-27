using System.Collections.Generic;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions.Actions.GameComponent;

[Function("GameComponent.SetCurrGeneralTime")]
public class GameComponentSetCurrGeneralTimeFunction(
	VirtualMachine vm,
	string[] parameters)
	: VmFunction {
	public override FunctionReturnType ReturnType => FunctionReturnType.Void;
	public override int ParamCount => 2;
	public FunctionSourceParam<GameTime>? Time { get; } = FunctionSourceParam<GameTime>.Read(parameters[0], vm);
	public FunctionSourceParam<bool>? SendTimerEvents { get; } = FunctionSourceParam<bool>.Read(parameters[1], vm);
	public override List<string>? GetParamStrings() => [Time.Write(), SendTimerEvents.Write()];
}
