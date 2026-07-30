using P2XMLEditor.GameData.Enums;
using System.Collections.Generic;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions.Actions.Storage;

[Function("Storage.ReceiveByTemplate")]
public class StorageReceiveByTemplateFunction(VirtualMachine vm, string[] parameters)
	: VmFunction {
	public override VmType ReturnType => VmType.Void;
	public override int ParamCount => 2;
	public FunctionSourceParam<EntityRef> Template { get; } = FunctionSourceParam<EntityRef>.Read(parameters[0], vm);
	public FunctionSourceParam<ObjRef> NewPlace { get; } = FunctionSourceParam<ObjRef>.Read(parameters[1], vm);
	public override List<string>? GetParamStrings() => [Template.Write(), NewPlace.Write()];
}