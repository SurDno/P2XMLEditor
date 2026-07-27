using System.Collections.Generic;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions.Actions.OutdoorCrowdComponent;

[Function("OutdoorCrowdComponent.AddEntity")]
public class OutdoorCrowdComponentAddEntityFunction(
	VirtualMachine vm,
	string[] parameters)
	: VmFunction {
	public override FunctionReturnType ReturnType => FunctionReturnType.Void;
	public override int ParamCount => 1;
	public FunctionSourceParam<EntityRef>? Entity { get; } = FunctionSourceParam<EntityRef>.Read(parameters[0], vm);
	public override List<string>? GetParamStrings() => [Entity.Write()];
}
