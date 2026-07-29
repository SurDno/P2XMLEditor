using P2XMLEditor.GameData.Enums;
using System.Collections.Generic;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions.Actions.LipSync;

[Function("LipSync.Play")]
public class LipSyncPlayFunction : VmFunction {
	public override VmType ReturnType => VmType.Void;
	public override int ParamCount => 1;
	public FunctionSourceParam<Sample>? Animation { get; }
	public override List<string>? GetParamStrings() => [Animation.Write()];
	public LipSyncPlayFunction(VirtualMachine vm, string[] parameters) {
		Animation = FunctionSourceParam<Sample>.Read(parameters[0], vm);
	}
}