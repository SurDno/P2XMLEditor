using System.Collections.Generic;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.Enums;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;
using P2XMLEditor.Helper;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions.Actions.GameComponent;

[Function("GameComponent.IsObjectCompatible")]
public class IsObjectCompatibleFunction : VmFunction {
	private readonly ParameterHolder holder;
	private readonly string message;
	private readonly VmTypeInfo componentInfo;
	private readonly Parameter? constParameter;
	public override FunctionReturnType ReturnType => FunctionReturnType.Bool;
	public override int ParamCount => 2;
	public IsObjectCompatibleFunction(VirtualMachine vm, string[] parameters) {
		var array = parameters[0].Split('%');
		holder = vm.GetElement<ParameterHolder>(ulong.Parse(array[0]));
		message = array[1];
		var text = parameters[1];
		var text2 = text;
		componentInfo = VmTypeHelper.GetVmTypeInfo(text2.Substring(1, text2.Length - 1), vm);
	}
	public override List<string>? GetParamStrings() => [$"{holder.Id}%{message}", "%" + componentInfo.Serialize()];
}
