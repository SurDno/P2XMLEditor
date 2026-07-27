using System.Collections.Generic;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions.Actions.Support;

[Function("Support.SetListValue")]
public class SupportSetListValueFunction : VmFunction {
	public override FunctionReturnType ReturnType => FunctionReturnType.Void;
	public override int ParamCount => 3;
	public FunctionSourceParam<CommonList>? ObjList { get; }
	public FunctionSourceParam<GameObject>? Val { get; }
	public FunctionSourceParam<int>? Index { get; }
	public override List<string>? GetParamStrings() => [ObjList.Write(), Val?.Write() ?? "", Index.Write()];
	public SupportSetListValueFunction(VirtualMachine vm, string[] parameters) {
		ObjList = FunctionSourceParam<CommonList>.Read((parameters.Length != 0) ? parameters[0] : "", vm);
		Val = FunctionSourceParam<GameObject>.Read(parameters[1], vm);
		Index = FunctionSourceParam<int>.Read((parameters.Length > 2) ? parameters[2] : "", vm);
	}
}
