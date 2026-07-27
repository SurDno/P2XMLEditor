using System.Collections.Generic;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions.Actions.Storage;

[Function("Storage.PickUpToInentoryByTemplate")]
public class StoragePickUpToInentoryByTemplateFunction(
	VirtualMachine vm,
	string[] parameters)
	: VmFunction {
	public override FunctionReturnType ReturnType => FunctionReturnType.Void;
	public override int ParamCount => 3;
	public FunctionSourceParam<EntityRef> Template { get; } = FunctionSourceParam<EntityRef>.Read(parameters[0], vm);
	public FunctionSourceParam<EntityRef> Container { get; } = FunctionSourceParam<EntityRef>.Read(parameters[1], vm);
	public FunctionSourceParam<int> Count { get; } = FunctionSourceParam<int>.Read(parameters[2], vm);
	public override List<string>? GetParamStrings() => [Template.Write(), Container.Write(), Count.Write()];
}
