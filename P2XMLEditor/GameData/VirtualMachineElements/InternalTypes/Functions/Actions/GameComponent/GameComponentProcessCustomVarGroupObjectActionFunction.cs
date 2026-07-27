using System.Collections.Generic;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions.Actions.GameComponent;

[Function("GameComponent.ProcessCustomVarGroupObjectAction")]
public class GameComponentProcessCustomVarGroupObjectActionFunction : VmFunction {
	public override FunctionReturnType ReturnType => FunctionReturnType.Void;
	public override int ParamCount => 6;
	public FunctionSourceParam<string>? GroupName { get; }
	public FunctionSourceParam<string>? ActionName { get; }
	public FunctionSourceParam<string>? ParamA { get; }
	public FunctionSourceParam<string>? ParamB { get; }
	public FunctionSourceParam<string>? ParamC { get; }
	public FunctionSourceParam<string>? Value { get; }
	public override List<string>? GetParamStrings() => [GroupName?.Write(), ActionName?.Write(), ParamA?.Write(), ParamB?.Write(), ParamC?.Write(), Value?.Write()];
	public GameComponentProcessCustomVarGroupObjectActionFunction(VirtualMachine vm, string[] parameters) {
		GroupName = FunctionSourceParam<string>.Read(parameters[0], vm);
		ActionName = FunctionSourceParam<string>.Read(parameters[1], vm);
		ParamA = FunctionSourceParam<string>.Read(parameters[2], vm);
		ParamB = FunctionSourceParam<string>.Read(parameters[3], vm);
		ParamC = FunctionSourceParam<string>.Read(parameters[4], vm);
		Value = FunctionSourceParam<string>.Read(parameters[5], vm);
	}
}
