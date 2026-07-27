using System.Collections.Generic;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions.Actions.GlobalStorageManager;

[Function("GlobalStorageManager.ResetTags")]
public class GlobalStorageManagerResetTagsFunction : VmFunction {
	public override FunctionReturnType ReturnType => FunctionReturnType.Void;
	public override int ParamCount => 1;
	public FunctionSourceParam<string> StorageGroup { get; }
	public override List<string>? GetParamStrings() => [StorageGroup.Write()];
	public GlobalStorageManagerResetTagsFunction(VirtualMachine vm, string[] parameters) {
		StorageGroup = FunctionSourceParam<string>.Read(parameters[0], vm);
	}
}
