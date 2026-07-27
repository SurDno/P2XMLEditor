using System.Collections.Generic;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions.Actions.GameComponent;

[Function("GameComponent.StartLoopGameTimerAtContext")]
public class GameComponentStartLoopGameTimerAtContextFunction(
	VirtualMachine vm,
	string[] parameters)
	: VmFunction {
	public override FunctionReturnType ReturnType => FunctionReturnType.ULong;
	public override int ParamCount => 2;
	public FunctionSourceParam<GameTime>? Interval { get; } = FunctionSourceParam<GameTime>.Read(parameters[0], vm);
	public FunctionSourceParam<GameMode>? GameMode { get; } = FunctionSourceParam<GameMode>.Read(parameters[1], vm);
	public override List<string>? GetParamStrings() => [Interval.Write(), GameMode.Write()];
}
