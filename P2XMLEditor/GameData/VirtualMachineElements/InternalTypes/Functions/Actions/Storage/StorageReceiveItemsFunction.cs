using System.Collections.Generic;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions.Actions.Storage;

[Function("Storage.ReceiveItems")]
public class StorageReceiveItemsFunction(VirtualMachine vm, string[] parameters)
	: VmFunction {
	public override FunctionReturnType ReturnType => FunctionReturnType.Void;
	public override int ParamCount => 2;
	public FunctionSourceParam<GameObject> NewPlace { get; } = FunctionSourceParam<GameObject>.Read((parameters.Length != 0) ? parameters[0] : "", vm);
	public FunctionSourceParam<int> ItemsCount { get; } = FunctionSourceParam<int>.Read((parameters.Length > 1) ? parameters[1] : "", vm);
	public override List<string>? GetParamStrings() => [NewPlace.Write(), ItemsCount.Write()];
}
