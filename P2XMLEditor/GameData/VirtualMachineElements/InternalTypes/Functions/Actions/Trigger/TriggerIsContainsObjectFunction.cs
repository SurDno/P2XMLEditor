using System.Collections.Generic;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;
using P2XMLEditor.Helper;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions.Actions.Trigger;

[Function("Trigger.IsContainsObject")]
public class TriggerIsContainsObjectFunction : VmFunction {
	private readonly GameObject gameObject;
	public override FunctionReturnType ReturnType => FunctionReturnType.Bool;
	public override int ParamCount => 1;
	public override List<string>? GetParamStrings() => [$"{gameObject.Id}%{gameObject.Id}"];
	public TriggerIsContainsObjectFunction(GameObject gameObject) {
		this.gameObject = gameObject;
	}
	public TriggerIsContainsObjectFunction(VirtualMachine vm, string[] parameters) {
		var array = parameters[0].Split('%');
		gameObject = vm.GetElement<GameObject>(ulong.Parse(array[0]));
	}
}
