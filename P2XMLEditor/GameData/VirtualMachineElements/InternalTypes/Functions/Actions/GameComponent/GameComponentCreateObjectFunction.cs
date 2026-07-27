using System.Collections.Generic;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions.Actions.GameComponent;

[Function("GameComponent.CreateObject")]
public class GameComponentCreateObjectFunction(VirtualMachine vm, string[] parameters)
	: VmFunction {
	public override FunctionReturnType ReturnType => FunctionReturnType.ObjRef;
	public override int ParamCount => 1;
	public FunctionSourceParam<BlueprintRef> Template { get; } = FunctionSourceParam<BlueprintRef>.Read(parameters[0], vm);
	public override List<string>? GetParamStrings() => [Template.Write()];
}
