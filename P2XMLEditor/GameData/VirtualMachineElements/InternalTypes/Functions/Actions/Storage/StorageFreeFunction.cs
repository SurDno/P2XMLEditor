using P2XMLEditor.GameData.Enums;
using System.Collections.Generic;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions.Actions.Storage;

[Function("Storage.Free")]
public class StorageFreeFunction : VmFunction {
	public override VmType ReturnType => VmType.Void;
	public override int ParamCount => 0;
	public override List<string>? GetParamStrings() => null;
	public StorageFreeFunction(VirtualMachine vm, string[] parameters) {
	}
}