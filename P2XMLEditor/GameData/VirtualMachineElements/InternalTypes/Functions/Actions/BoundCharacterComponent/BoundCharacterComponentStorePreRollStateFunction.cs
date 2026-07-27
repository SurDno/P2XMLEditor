using System.Collections.Generic;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions.Actions.BoundCharacterComponent;

[Function("BoundCharacterComponent.StorePreRollState")]
public class BoundCharacterComponentStorePreRollStateFunction : VmFunction {
	public override FunctionReturnType ReturnType => FunctionReturnType.Void;
	public override int ParamCount => 0;
	public override List<string>? GetParamStrings() => null;
	public BoundCharacterComponentStorePreRollStateFunction(VirtualMachine vm, string[] parameters) {
	}
}
