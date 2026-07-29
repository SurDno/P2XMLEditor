using P2XMLEditor.GameData.Enums;
using System.Collections.Generic;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions.Actions.GlobalStorageManager;

[Function("GlobalStorageManager.SetAllStorablesDescription")]
public class GlobalStorageManagerSetAllStorablesDescriptionFunction : VmFunction {
	public override VmType ReturnType => VmType.Void;
	public override int ParamCount => 2;
	public FunctionSourceParam<EntityRef>? StorageGroup { get; }
	public FunctionSourceParam<GameString>? Value { get; }
	public override List<string>? GetParamStrings() => [StorageGroup?.Write(), Value?.Write()];
	public GlobalStorageManagerSetAllStorablesDescriptionFunction(VirtualMachine vm, string[] parameters) {
		StorageGroup = FunctionSourceParam<EntityRef>.Read(parameters[0], vm);
		Value = FunctionSourceParam<GameString>.Read(parameters[1], vm);
	}
}