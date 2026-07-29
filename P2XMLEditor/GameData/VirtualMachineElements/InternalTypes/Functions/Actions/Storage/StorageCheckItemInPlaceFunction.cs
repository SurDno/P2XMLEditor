using P2XMLEditor.GameData.Enums;
using System.Collections.Generic;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions.Actions.Storage;

[Function("Storage.CheckItemInPlace")]
public class StorageCheckItemInPlaceFunction : VmFunction {
	public override VmType ReturnType => VmType.Boolean;
	public override int ParamCount => 2;
	public FunctionSourceParam<EntityRef>? Template { get; }
	public FunctionSourceParam<EntityRef>? ContainerTemplate { get; }
	public override List<string>? GetParamStrings() => [Template?.Write() ?? "", ContainerTemplate?.Write() ?? ""];
	public StorageCheckItemInPlaceFunction(VirtualMachine vm, string[] parameters) {
		Template = FunctionSourceParam<EntityRef>.Read(parameters[0], vm);
		ContainerTemplate = FunctionSourceParam<EntityRef>.Read(parameters[1], vm);
	}
}