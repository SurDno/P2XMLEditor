using System.Collections.Generic;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions.Actions.GameComponent;

[Function("GameComponent.GetCurrentGameTimeContext")]
public class GameComponentGetCurrentGameTimeContextFunction : VmFunction {
	public override FunctionReturnType ReturnType => FunctionReturnType.GameMode;
	public override int ParamCount => 0;
	public override List<string>? GetParamStrings() => null;
	public GameComponentGetCurrentGameTimeContextFunction(VirtualMachine vm, string[] parameters) {
	}
}
