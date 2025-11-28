using System;
using System.Collections.Generic;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;
using P2XMLEditor.Helper;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions;

[Function("GameComponent.IsObjectExist")]
public class IsObjectExistFunction : VmFunction {
	public override FunctionReturnType ReturnType => FunctionReturnType.Bool;
	public override int ParamCount => 1;
   
	private readonly ParameterHolder holder;
	private readonly string? message;
   
	public IsObjectExistFunction(ParameterHolder holder, string? message = null) {
		this.holder = holder ?? throw new ArgumentNullException(nameof(holder));
		this.message = message;
	}
   
	public IsObjectExistFunction(VirtualMachine vm, List<string> parameters)  {
		if (parameters.Count != 1)
			throw new ArgumentException($"Expected 1 parameter, got {parameters.Count}");
           
		var parts = parameters[0].Split('%', 2);
       
		holder = vm.GetElement<ParameterHolder>(ulong.Parse(parts[0]));
		message = parts.Length > 1 ? parts[1] : null;
	}
   
	public override List<string> GetParamStrings() => [message != null ? $"{holder.Id}%{message}" : holder.Id.ToString()];
}