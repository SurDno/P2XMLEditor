using P2XMLEditor.GameData.Enums;
using System.Collections.Generic;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions.Actions.GameComponent;

[Function("GameComponent.GetGameLocalization")]
public class GameComponentGetGameLocalizationFunction : VmFunction {
	public override VmType ReturnType => VmType.GameLocalizationName;
	public override int ParamCount => 0;
	public override List<string>? GetParamStrings() => null;
	public GameComponentGetGameLocalizationFunction(VirtualMachine vm, string[] parameters) {
	}
}