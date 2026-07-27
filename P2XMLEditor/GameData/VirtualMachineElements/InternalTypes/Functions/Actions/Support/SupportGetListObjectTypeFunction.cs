using System.Collections.Generic;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions.Actions.Support;

[Function("Support.GetListObjectType")]
public class SupportGetListObjectTypeFunction(VirtualMachine vm, string[] parameters)
	: VmFunction {
	public override FunctionReturnType ReturnType => FunctionReturnType.Object;
	public override int ParamCount => 2;
	public FunctionSourceParam<CommonList>? ObjList { get; } = FunctionSourceParam<CommonList>.Read((parameters.Length != 0) ? parameters[0] : "", vm);
	public FunctionSourceParam<int>? Index { get; } = FunctionSourceParam<int>.Read((parameters.Length > 1) ? parameters[1] : "", vm);
	public override List<string>? GetParamStrings() => [ObjList.Write(), Index.Write()];
}
