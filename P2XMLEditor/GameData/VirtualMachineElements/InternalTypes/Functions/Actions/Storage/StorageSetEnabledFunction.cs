using System.Collections.Generic;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions.Actions.Storage;

[Function("Storage.SetEnabled")]
public class StorageSetEnabledFunction(VirtualMachine vm, string[] parameters)
	: VmFunction {
	public override FunctionReturnType ReturnType => FunctionReturnType.Void;
	public override int ParamCount => 2;
	public FunctionSourceParam<EntityRef> Storage { get; } = FunctionSourceParam<EntityRef>.Read((parameters.Length != 0) ? parameters[0] : "", vm);
	public FunctionSourceParam<bool> Enabled { get; } = FunctionSourceParam<bool>.Read((parameters.Length > 1) ? parameters[1] : "", vm);
	public override List<string>? GetParamStrings() => [Storage.Write(), Enabled.Write()];
}
