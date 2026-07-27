using System.Collections.Generic;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions.Actions.Storage;

[Function("Storage.SetAllAvailable")]
public class StorageSetAllAvailableFunction : VmFunction {
	public override FunctionReturnType ReturnType => FunctionReturnType.Void;
	public override int ParamCount => 1;
	public FunctionSourceParam<bool>? Available { get; }
	public override List<string>? GetParamStrings() => [Available?.Write() ?? ""];
	public StorageSetAllAvailableFunction(VirtualMachine vm, string[] parameters) {
		Available = FunctionSourceParam<bool>.Read(parameters[0], vm);
	}
}
