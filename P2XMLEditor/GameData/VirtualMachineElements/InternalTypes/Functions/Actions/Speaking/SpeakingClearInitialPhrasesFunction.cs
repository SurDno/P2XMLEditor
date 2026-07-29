using P2XMLEditor.GameData.Enums;
using System.Collections.Generic;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions.Actions.Speaking;

[Function("Speaking.ClearInitialPhrases")]
public class SpeakingClearInitialPhrasesFunction : VmFunction {
	public override VmType ReturnType => VmType.Void;
	public override int ParamCount => 0;
	public override List<string>? GetParamStrings() => null;
	public SpeakingClearInitialPhrasesFunction(VirtualMachine vm, string[] parameters) {
	}
}