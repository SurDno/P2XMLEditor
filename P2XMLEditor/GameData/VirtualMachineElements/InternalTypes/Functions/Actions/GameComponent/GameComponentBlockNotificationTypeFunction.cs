using System.Collections.Generic;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.Enums;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions.Actions.GameComponent;

[Function("GameComponent.BlockNotificationType")]
public class GameComponentBlockNotificationTypeFunction(
	VirtualMachine vm,
	string[] parameters)
	: VmFunction {
	public override FunctionReturnType ReturnType => FunctionReturnType.Void;
	public override int ParamCount => 1;
	public FunctionSourceParam<NotificationType> Notification { get; } = FunctionSourceParam<NotificationType>.Read(parameters[0], vm);
	public override List<string>? GetParamStrings() => [Notification.Write()];
}
