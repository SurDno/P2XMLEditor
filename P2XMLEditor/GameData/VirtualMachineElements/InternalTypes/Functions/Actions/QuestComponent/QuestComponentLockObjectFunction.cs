using System.Collections.Generic;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions.Actions.QuestComponent;

[Function("QuestComponent.LockObject")]
public class QuestComponentLockObjectFunction : VmFunction {
	public override FunctionReturnType ReturnType => FunctionReturnType.Void;
	public override int ParamCount => 1;
	public override List<string>? GetParamStrings() => null;
	public QuestComponentLockObjectFunction(VirtualMachine vm, string[] parameters) {
	}
}
