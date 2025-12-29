using System;
using System.Collections.Generic;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions;

[Function("Support.Random")]
public class RandomFunction : VmFunction {
	public override FunctionReturnType ReturnType => FunctionReturnType.Float;
	public override int ParamCount => 0;
   
	public RandomFunction() {}
   
	public RandomFunction(VirtualMachine vm, string[] parameters) {
		if (parameters.Length != 0)
			throw new ArgumentException($"Expected no parameters, got {parameters.Length}");
	}
   
	public override List<string>? GetParamStrings() => null;
}