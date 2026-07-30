using P2XMLEditor.GameData.Enums;
using System.Collections.Generic;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions.Actions.QuestComponent;

[Function("QuestComponent.LockObject")]
public class QuestComponentLockObjectFunction : VmFunction {
	public override VmType ReturnType => VmType.Void;
	public override int ParamCount => 1;
	public FunctionSourceParam<ObjRef> Object { get; }
	public override List<string>? GetParamStrings() => [Object.Write()];
	public QuestComponentLockObjectFunction(VirtualMachine vm, string[] parameters) {
		Object = FunctionSourceParam<ObjRef>.Read(parameters[0], vm);
	}
}