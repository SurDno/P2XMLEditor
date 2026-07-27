using System.Collections.Generic;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.Enums;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions.Actions.GameComponent;

[Function("GameComponent.AddNotificationTemplate")]
public class GameComponentAddNotificationTemplateFunction(
	VirtualMachine vm,
	string[] parameters)
	: VmFunction {
	public override FunctionReturnType ReturnType => FunctionReturnType.Void;
	public override int ParamCount => 2;
	public FunctionSourceParam<NotificationType> Notification { get; } = FunctionSourceParam<NotificationType>.Read(parameters[0], vm);
	public FunctionSourceParam<EntityRef> TextTemplateRef { get; } = FunctionSourceParam<EntityRef>.Read(parameters[1], vm);
	public override List<string>? GetParamStrings() => [Notification.Write(), TextTemplateRef.Write()];
}
