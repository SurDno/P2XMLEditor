using P2XMLEditor.GameData.Enums;
using System.Collections.Generic;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions.Actions.Storage;

[Function("Storage.GetOpen")]
public class StorageGetOpenFunction(VirtualMachine vm, string[] parameters)
	: VmFunction {
	public override VmType ReturnType => VmType.Int32;
	public override int ParamCount => 1;
	public FunctionSourceParam<EntityRef> ContainerTemplate { get; } = FunctionSourceParam<EntityRef>.Read(parameters[0], vm);
	public override List<string>? GetParamStrings() => [ContainerTemplate.Write()];
}