using P2XMLEditor.GameData.Enums;
using System.Collections.Generic;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions.Actions.Storage;

[Function("Storage.GetItemsCount")]
public class StorageGetItemsCountFunction : VmFunction {
	public override VmType ReturnType => VmType.Int32;
	public override int ParamCount => 0;
	public StorageGetItemsCountFunction() {
	}
	public StorageGetItemsCountFunction(VirtualMachine vm, string[] parameters) {
	}
	public override List<string>? GetParamStrings() => null;
}