using P2XMLEditor.GameData.Enums;
using System.Collections.Generic;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions.Actions.Support;

[Function("Support.SetListValue")]
public class SupportSetListValueFunction : VmFunction {
	public override VmType ReturnType => VmType.Void;
	public override int ParamCount => 3;
	public FunctionSourceParam<CommonList>? ObjList { get; }
	public FunctionSourceParam<object>? Val { get; }
	public FunctionSourceParam<int>? Index { get; }

	public SupportSetListValueFunction(VirtualMachine vm, string[] parameters) {
		ObjList = FunctionSourceParam<CommonList>.Read(parameters[0], vm);
		Val = FunctionSourceParam<object>.Read(parameters[1], vm, CommonList.GetElementType(ObjList, vm));
		Index = FunctionSourceParam<int>.Read(parameters[2], vm);
	}

	public override List<string>? GetParamStrings() => [ObjList.Write(), Val?.Write() ?? "", Index.Write()];
}