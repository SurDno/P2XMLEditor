using System.Collections.Generic;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions.Actions.Speaking;

[Function("Speaking.RemoveInitialPhrase")]
public class SpeakingRemoveInitialPhraseFunction : VmFunction {
	public override FunctionReturnType ReturnType => FunctionReturnType.Void;
	public override int ParamCount => 1;
	public FunctionSourceParam<Sample>? LipSync { get; }
	public override List<string>? GetParamStrings() => [LipSync?.Write() ?? ""];
	public SpeakingRemoveInitialPhraseFunction(VirtualMachine vm, string[] parameters) {
		LipSync = FunctionSourceParam<Sample>.Read(parameters[0], vm);
	}
}
