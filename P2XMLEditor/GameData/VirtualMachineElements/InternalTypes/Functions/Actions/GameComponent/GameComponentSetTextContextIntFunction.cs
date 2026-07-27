using System.Collections.Generic;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions.Actions.GameComponent;

[Function("GameComponent.SetTextContextInt")]
public class GameComponentSetTextContextIntFunction(
	VirtualMachine vm,
	string[] parameters)
	: VmFunction {
	public override FunctionReturnType ReturnType => FunctionReturnType.Void;
	public override int ParamCount => 2;
	public FunctionSourceParam<string>? Tag { get; } = FunctionSourceParam<string>.Read(parameters[0], vm);
	public FunctionSourceParam<int>? Value { get; } = FunctionSourceParam<int>.Read(parameters[1], vm);
	public override List<string>? GetParamStrings() => [Tag.Write(), Value.Write()];
}
