using System;
using System.Collections.Generic;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;
using P2XMLEditor.Helper;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions.Actions.GameComponent;

[Function("GameComponent.GetObjectClass")]
public class GetObjectClassFunction : VmFunction {
	private readonly ParameterHolder holder;
	private readonly string? messageParam;
	public override FunctionReturnType ReturnType => FunctionReturnType.BlueprintRef;
	public override int ParamCount => 1;
	public GetObjectClassFunction(ParameterHolder holder, string? messageParam = null) {
		this.holder = holder ?? throw new ArgumentNullException("holder");
		this.messageParam = messageParam;
	}
	public GetObjectClassFunction(VirtualMachine vm, string[] parameters) {
		var array = parameters[0].Split('%', 2);
		holder = vm.GetElement<ParameterHolder>(ulong.Parse(array[0]));
		messageParam = ((array.Length > 1) ? array[1] : null);
	}
	public override List<string>? GetParamStrings() => [((messageParam != null) ? $"{holder.Id}%{messageParam}" : holder.Id.ToString())];
}
