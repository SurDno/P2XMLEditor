using System.Collections.Generic;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.Enums;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions.Actions.GameComponent;

[Function("GameComponent.AddNotification")]
public class GameComponentAddNotificationFunction(
	VirtualMachine vm,
	string[] parameters)
	: VmFunction {
	public override VmType ReturnType => VmType.Void;
	public override int ParamCount => 2;
	public FunctionSourceParam<NotificationType>? Notification { get; } = FunctionSourceParam<NotificationType>.Read(parameters[0], vm);
	public FunctionSourceParam<CommonList>? TextList { get; } = FunctionSourceParam<CommonList>.Read(parameters[1], vm);
	public override List<string>? GetParamStrings() => [Notification.Write(), TextList.Write()];
}