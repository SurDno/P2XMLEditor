using System.Collections.Generic;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.Enums;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions.Actions.Storage;

[Function("Storage.SetAllOpen")]
public class StorageSetAllOpenFunction(VirtualMachine vm, string[] parameters)
	: VmFunction {
	public override VmType ReturnType => VmType.Void;
	public override int ParamCount => 1;
	public FunctionSourceParam<ContainerOpenState> State { get; } = FunctionSourceParam<ContainerOpenState>.Read(parameters[0], vm);
	public override List<string>? GetParamStrings() => [State.Write()];
}