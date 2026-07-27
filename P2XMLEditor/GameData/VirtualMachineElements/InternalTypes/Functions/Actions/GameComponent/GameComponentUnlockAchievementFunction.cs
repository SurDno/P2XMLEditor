using System.Collections.Generic;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions.Actions.GameComponent;

[Function("GameComponent.UnlockAchievement")]
public class GameComponentUnlockAchievementFunction(
	VirtualMachine vm,
	string[] parameters)
	: VmFunction {
	public override FunctionReturnType ReturnType => FunctionReturnType.Void;
	public override int ParamCount => 1;
	public FunctionSourceParam<string> Id { get; } = FunctionSourceParam<string>.Read(parameters[0], vm);
	public override List<string>? GetParamStrings() => [Id.Write()];
}
