using System.Collections.Generic;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.Enums;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions.Actions.Position;

[Function("Position.TeleportToArea")]
public class PositionTeleportToAreaFunction(VirtualMachine vm, string[] parameters)
	: VmFunction {
	public override VmType ReturnType => VmType.Void;
	public override int ParamCount => 2;
	public FunctionSourceParam<EntityRef> Target { get; } = FunctionSourceParam<EntityRef>.Read(parameters[0], vm);
	public FunctionSourceParam<Area> Area { get; } = FunctionSourceParam<Area>.Read(parameters[1], vm);
	public override List<string>? GetParamStrings() => [Target.Write(), Area.Write()];
}