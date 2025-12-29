using System;
using System.Collections.Generic;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions;

[Function("GameComponent.GetCurrentGameTimeContext")]
public class GetCurrentGameTimeContextFunction : VmFunction {
	public override FunctionReturnType ReturnType => FunctionReturnType.GameMode;
	public override int ParamCount => 0;
   
	public GetCurrentGameTimeContextFunction() {}
   
	public GetCurrentGameTimeContextFunction(VirtualMachine vm, string[] parameters) {
		if (parameters.Length != 0)
			throw new ArgumentException($"Expected no parameters, got {parameters.Length}");
	}

	public override List<string>? GetParamStrings() => null;
}