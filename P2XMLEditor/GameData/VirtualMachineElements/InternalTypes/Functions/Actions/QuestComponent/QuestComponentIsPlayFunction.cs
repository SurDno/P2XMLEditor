using P2XMLEditor.GameData.Enums;
using System.Collections.Generic;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions.Actions.QuestComponent;

[Function("QuestComponent.IsPlay")]
public class QuestComponentIsPlayFunction : VmFunction {
	public override VmType ReturnType => VmType.Boolean;
	public override int ParamCount => 0;
	public override List<string>? GetParamStrings() => null;
	public QuestComponentIsPlayFunction(VirtualMachine vm, string[] parameters) {
	}
}