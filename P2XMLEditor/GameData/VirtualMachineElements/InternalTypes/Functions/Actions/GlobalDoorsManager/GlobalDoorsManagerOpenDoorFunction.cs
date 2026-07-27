using System.Collections.Generic;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions.Actions.GlobalDoorsManager;

[Function("GlobalDoorsManager.OpenDoor")]
public class GlobalDoorsManagerOpenDoorFunction(VirtualMachine vm, string[] parameters)
	: VmFunction {
	public override FunctionReturnType ReturnType => FunctionReturnType.Void;
	public override int ParamCount => 1;
	public FunctionSourceParam<GameObject> GateObj { get; } = FunctionSourceParam<GameObject>.Read(parameters[0], vm);
	public override List<string>? GetParamStrings() => [GateObj.Write()];
}
