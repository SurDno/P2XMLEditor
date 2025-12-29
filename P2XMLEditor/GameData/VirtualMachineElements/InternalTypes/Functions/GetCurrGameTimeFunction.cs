using System;
using System.Collections.Generic;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions;

[Function("GameComponent.GetCurrGameTime")]
public class GetCurrGameTimeFunction : VmFunction {
	public override FunctionReturnType ReturnType => FunctionReturnType.GameTime;
	public override int ParamCount => 0;
   
	public GetCurrGameTimeFunction() {}
   
	public GetCurrGameTimeFunction(VirtualMachine vm, string[] parameters) {
		if (parameters.Length != 0)
			throw new ArgumentException($"Expected no parameters, got {parameters.Length}");
	}
   
	public override List<string>? GetParamStrings() => null;
}