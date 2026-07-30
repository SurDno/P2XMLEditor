using System.Collections.Generic;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.Enums;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;
using P2XMLEditor.Helper;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions.Actions.GameComponent;

[Function("GameComponent.IsObjectCompatible")]
public class GameComponentIsObjectCompatibleFunction(VirtualMachine vm, string[] parameters) : VmFunction {
	public override VmType ReturnType => VmType.Boolean;
	public override int ParamCount => 2;
	public FunctionSourceParam<ObjRef>? Object { get; } = FunctionSourceParam<ObjRef>.Read(parameters[0], vm);
	public FunctionSourceParam<VmTypeInfo>? Type { get; } = FunctionSourceParam<VmTypeInfo>.Read(parameters[1], vm, VmType.TypeValue);
	public override List<string>? GetParamStrings() => [Object.Write(), Type.Write()];
}