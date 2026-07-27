using System.Collections.Generic;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions.Actions.GameComponent;

[Function("GameComponent.StopTimerAtContext")]
public class GameComponentStopTimerAtContextFunction(
	VirtualMachine vm,
	string[] parameters)
	: VmFunction {
	public override FunctionReturnType ReturnType => FunctionReturnType.Void;
	public override int ParamCount => 2;
	public FunctionSourceParam<ulong>? TimerIndex { get; } = FunctionSourceParam<ulong>.Read(parameters[0], vm);
	public FunctionSourceParam<GameMode>? GameMode { get; } = FunctionSourceParam<GameMode>.Read(parameters[1], vm);
	public override List<string>? GetParamStrings() => [TimerIndex.Write(), GameMode.Write()];
}
