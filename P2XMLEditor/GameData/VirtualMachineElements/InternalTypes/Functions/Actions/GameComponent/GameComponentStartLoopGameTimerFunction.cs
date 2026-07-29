using P2XMLEditor.GameData.Enums;
using System.Collections.Generic;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions.Actions.GameComponent;

[Function("GameComponent.StartLoopGameTimer")]
public class GameComponentStartLoopGameTimerFunction(
	VirtualMachine vm,
	string[] parameters)
	: VmFunction {
	public override VmType ReturnType => VmType.UInt64;
	public override int ParamCount => 1;
	public FunctionSourceParam<GameTime>? Interval { get; } = FunctionSourceParam<GameTime>.Read(parameters[0], vm);
	public override List<string>? GetParamStrings() => [Interval.Write()];
}