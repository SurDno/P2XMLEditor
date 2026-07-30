using P2XMLEditor.GameData.Enums;
using System.Collections.Generic;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions.Actions.Storage;

[Function("Storage.GetEnabled")]
public class StorageGetEnabledFunction(VirtualMachine vm, string[] parameters)
	: VmFunction {
	public override VmType ReturnType => VmType.Boolean;
	public override int ParamCount => 1;
	public FunctionSourceParam<EntityRef> ContainerTemplate { get; } = FunctionSourceParam<EntityRef>.Read(parameters[0], vm);
	public override List<string>? GetParamStrings() => [ContainerTemplate.Write()];
}