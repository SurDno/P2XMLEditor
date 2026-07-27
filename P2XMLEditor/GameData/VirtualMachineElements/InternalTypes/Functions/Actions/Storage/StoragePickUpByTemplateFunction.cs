using System.Collections.Generic;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions.Actions.Storage;

[Function("Storage.PickUpByTemplate")]
public class StoragePickUpByTemplateFunction(VirtualMachine vm, string[] parameters)
	: VmFunction {
	public override FunctionReturnType ReturnType => FunctionReturnType.Void;
	public override int ParamCount => 1;
	public FunctionSourceParam<EntityRef> Template { get; } = FunctionSourceParam<EntityRef>.Read(parameters[0], vm);
	public override List<string>? GetParamStrings() => [Template.Write()];
}
