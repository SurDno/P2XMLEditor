using System.Collections.Generic;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.Enums;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions.Actions.GameComponent;

[Function("GameComponent.AddNotification_v1")]
public class GameComponentAddNotification_v1Function : VmFunction {
	public override FunctionReturnType ReturnType => FunctionReturnType.Void;
	public override int ParamCount => 2;
	public FunctionSourceParam<NotificationType> Notification { get; }
	public CommonVariable? TextList { get; }
	public CommonVariable? ParamsList { get; }
	public override List<string>? GetParamStrings() => [Notification.Write(), TextList.Write()];
	public GameComponentAddNotification_v1Function(VirtualMachine vm, string[] parameters) {
		Notification = FunctionSourceParam<NotificationType>.Read(parameters[0], vm);
		TextList = CommonVariable.Read(parameters[1], vm);
		ParamsList = CommonVariable.Read(parameters[2], vm);
	}
}
