using P2XMLEditor.GameData.Enums;
using System.Collections.Generic;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions.Actions.GameComponent;

[Function("GameComponent.SetProfileValue")]
public class GameComponentSetProfileValueFunction(
	VirtualMachine vm,
	string[] parameters)
	: VmFunction {
	public override VmType ReturnType => VmType.Void;
	public override int ParamCount => 2;
	public FunctionSourceParam<string>? ProfileName { get; } = FunctionSourceParam<string>.Read(parameters[0], vm);
	public FunctionSourceParam<string>? ProfileValue { get; } = FunctionSourceParam<string>.Read(parameters[1], vm);
	public override List<string>? GetParamStrings() => [ProfileName?.Write() ?? "", ProfileValue?.Write() ?? ""];
}