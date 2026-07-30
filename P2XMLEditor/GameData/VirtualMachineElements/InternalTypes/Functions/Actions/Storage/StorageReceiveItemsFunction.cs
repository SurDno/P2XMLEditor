using P2XMLEditor.GameData.Enums;
using System.Collections.Generic;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions.Actions.Storage;

[Function("Storage.ReceiveItems")]
public class StorageReceiveItemsFunction(VirtualMachine vm, string[] parameters)
	: VmFunction {
	public override VmType ReturnType => VmType.Void;
	public override int ParamCount => 2;
	public FunctionSourceParam<ObjRef> NewPlace { get; } = FunctionSourceParam<ObjRef>.Read(parameters[0], vm);
	public FunctionSourceParam<int> ItemsCount { get; } = FunctionSourceParam<int>.Read(parameters[1], vm);
	public override List<string>? GetParamStrings() => [NewPlace.Write(), ItemsCount.Write()];
}