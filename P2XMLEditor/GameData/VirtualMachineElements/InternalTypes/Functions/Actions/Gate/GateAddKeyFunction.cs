using System.Collections.Generic;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions.Actions.Gate;

[Function("Gate.AddKey")]
public class GateAddKeyFunction(VirtualMachine vm, string[] parameters)
	: VmFunction {
	public override FunctionReturnType ReturnType => FunctionReturnType.Void;
	public override int ParamCount => 1;
	public FunctionSourceParam<EntityRef> Storable { get; } = FunctionSourceParam<EntityRef>.Read(parameters[0], vm);
	public override List<string>? GetParamStrings() => [Storable.Write()];
}
