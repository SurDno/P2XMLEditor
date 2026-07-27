using System.Collections.Generic;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions.Actions.AttackerPlayer;

[Function("AttackerPlayer.WeaponFirearmUnholster")]
public class AttackerPlayerWeaponFirearmUnholsterFunction : VmFunction {
	public override FunctionReturnType ReturnType => FunctionReturnType.Void;
	public override int ParamCount => 0;
	public override List<string>? GetParamStrings() => null;
	public AttackerPlayerWeaponFirearmUnholsterFunction(VirtualMachine vm, string[] parameters) {
	}
}
