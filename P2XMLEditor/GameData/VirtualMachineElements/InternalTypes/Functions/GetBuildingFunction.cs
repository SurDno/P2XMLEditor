using System;
using System.Collections.Generic;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions;

[Function("Position.GetBuilding")]
public class GetBuildingFunction : VmFunction {
	public override FunctionReturnType ReturnType => FunctionReturnType.Entity;
	public override int ParamCount => 0;
   
	public GetBuildingFunction() {}
   
	public GetBuildingFunction(VirtualMachine vm, string[] parameters) {
		if (parameters.Length != 0)
			throw new ArgumentException($"Expected no parameters, got {parameters.Length}");
	}

	public override List<string>? GetParamStrings() => null;
}