using System.Collections.Generic;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions.Actions.Speaking;

[Function("Speaking.AddForcedDialog")]
public class SpeakingAddForcedDialogFunction(VirtualMachine vm, string[] parameters)
	: VmFunction {
	public override FunctionReturnType ReturnType => FunctionReturnType.Void;
	public override int ParamCount => 1;
	public FunctionSourceParam<float> Distance { get; } = FunctionSourceParam<float>.Read(parameters[0], vm);
	public override List<string>? GetParamStrings() => [Distance.Write()];
}
