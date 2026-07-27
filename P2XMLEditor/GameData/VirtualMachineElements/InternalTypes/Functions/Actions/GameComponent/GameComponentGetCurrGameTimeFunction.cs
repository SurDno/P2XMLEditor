using System.Collections.Generic;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions.Actions.GameComponent;

[Function("GameComponent.GetCurrGameTime")]
public class GameComponentGetCurrGameTimeFunction : VmFunction {
	public override FunctionReturnType ReturnType => FunctionReturnType.GameTime;
	public override int ParamCount => 0;
	public override List<string>? GetParamStrings() => null;
	public GameComponentGetCurrGameTimeFunction(VirtualMachine vm, string[] parameters) {
	}
}
