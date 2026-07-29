using P2XMLEditor.GameData.Enums;
using System.Collections.Generic;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions.Actions.QuestComponent;

[Function("QuestComponent.UnLockObject")]
public class QuestComponentUnLockObjectFunction : VmFunction {
	public override VmType ReturnType => VmType.Void;
	public override int ParamCount => 1;
	public override List<string>? GetParamStrings() => null;
	public QuestComponentUnLockObjectFunction(VirtualMachine vm, string[] parameters) {
	}
}