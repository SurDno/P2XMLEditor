using System.Collections.Generic;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions.Actions.Storage;

[Function("Storage.SetStorablesTitle")]
public class StorageSetStorablesTitleFunction(VirtualMachine vm, string[] parameters)
	: VmFunction {
	public override FunctionReturnType ReturnType => FunctionReturnType.Void;
	public override int ParamCount => 2;
	public FunctionSourceParam<EntityRef>? Template { get; } = FunctionSourceParam<EntityRef>.Read((parameters.Length != 0) ? parameters[0] : "", vm);
	public FunctionSourceParam<GameString>? Text { get; } = FunctionSourceParam<GameString>.Read((parameters.Length > 1) ? parameters[1] : "", vm);
	public override List<string>? GetParamStrings() => [Template.Write(), Text.Write()];
}
