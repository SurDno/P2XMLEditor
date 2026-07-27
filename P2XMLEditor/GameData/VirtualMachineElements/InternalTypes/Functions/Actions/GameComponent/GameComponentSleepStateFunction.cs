using System.Collections.Generic;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions.Actions.GameComponent;

[Function("GameComponent.SleepState")]
public class GameComponentSleepStateFunction(VirtualMachine vm, string[] parameters)
	: VmFunction {
	public override FunctionReturnType ReturnType => FunctionReturnType.ULong;
	public override int ParamCount => 1;
	public FunctionSourceParam<GameTime>? Interval { get; } = FunctionSourceParam<GameTime>.Read(parameters[0], vm);
	public override List<string>? GetParamStrings() => [Interval.Write()];
}
