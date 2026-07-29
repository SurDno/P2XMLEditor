using P2XMLEditor.GameData.Enums;
using System.Collections.Generic;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions.Actions.GlobalStorageManager;

[Function("GlobalStorageManager.FreeStorages")]
public class GlobalStorageManagerFreeStoragesFunction : VmFunction {
	public override VmType ReturnType => VmType.Void;
	public override int ParamCount => 3;
	public FunctionSourceParam<string>? StorageGroup { get; }
	public FunctionSourceParam<string>? Probability { get; }
	public FunctionSourceParam<string>? Source { get; }
	public override List<string>? GetParamStrings() => [StorageGroup?.Write() ?? "", Probability?.Write() ?? "", Source?.Write() ?? ""];
	public GlobalStorageManagerFreeStoragesFunction(VirtualMachine vm, string[] parameters) {
		StorageGroup = FunctionSourceParam<string>.Read(parameters[0], vm);
		Probability = FunctionSourceParam<string>.Read(parameters[1], vm);
		Source = FunctionSourceParam<string>.Read(parameters[2], vm);
	}
}