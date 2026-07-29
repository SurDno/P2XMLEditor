using P2XMLEditor.GameData.Enums;
using System.Collections.Generic;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions.Actions.AttackerPlayer;

[Function("AttackerPlayer.WeaponMeleeUnholster")]
public class AttackerPlayerWeaponMeleeUnholsterFunction : VmFunction {
	public override VmType ReturnType => VmType.Void;
	public override int ParamCount => 0;
	public override List<string>? GetParamStrings() => null;
	public AttackerPlayerWeaponMeleeUnholsterFunction(VirtualMachine vm, string[] parameters) {
	}
}