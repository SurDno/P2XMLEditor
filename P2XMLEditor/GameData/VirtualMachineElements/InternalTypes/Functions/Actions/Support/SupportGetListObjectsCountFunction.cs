using P2XMLEditor.GameData.Enums;
using System.Collections.Generic;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions.Actions.Support;

[Function("Support.GetListObjectsCount")]
public class SupportGetListObjectsCountFunction : VmFunction {
	public override VmType ReturnType => VmType.Int32;
	public override int ParamCount => 1;
	public FunctionSourceParam<CommonList>? List { get; }
	public override List<string>? GetParamStrings() => [List?.Write() ?? ""];
	public SupportGetListObjectsCountFunction(VirtualMachine vm, string[] parameters) {
		List = FunctionSourceParam<CommonList>.Read(parameters[0], vm);
	}
}