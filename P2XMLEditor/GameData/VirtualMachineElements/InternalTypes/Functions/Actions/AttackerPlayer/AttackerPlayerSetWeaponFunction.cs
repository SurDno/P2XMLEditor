using System.Collections.Generic;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.Enums;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions.Actions.AttackerPlayer;

[Function("AttackerPlayer.SetWeapon")]
public class AttackerPlayerSetWeaponFunction(VirtualMachine vm, string[] parameters)
	: VmFunction {
	public override FunctionReturnType ReturnType => FunctionReturnType.Void;
	public override int ParamCount => 1;
	public FunctionSourceParam<WeaponKind> Weapon { get; } = FunctionSourceParam<WeaponKind>.Read(parameters[0], vm);
	public override List<string>? GetParamStrings() => [Weapon.Write()];
}
