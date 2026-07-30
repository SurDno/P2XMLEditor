using P2XMLEditor.GameData.Enums;
using System.Collections.Generic;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions.Actions.GlobalDoorsManager;

[Function("GlobalDoorsManager.IsDoorOpen")]
public class GlobalDoorsManagerIsDoorOpenFunction(
	VirtualMachine vm,
	string[] parameters)
	: VmFunction {
	public override VmType ReturnType => VmType.Boolean;
	public override int ParamCount => 1;
	public FunctionSourceParam<ObjRef> GateObj { get; } = FunctionSourceParam<ObjRef>.Read(parameters[0], vm);
	public override List<string>? GetParamStrings() => [GateObj.Write()];
}