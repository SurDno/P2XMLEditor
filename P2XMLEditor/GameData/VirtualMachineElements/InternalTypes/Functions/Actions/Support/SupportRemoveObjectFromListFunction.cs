using P2XMLEditor.GameData.Enums;
using System.Collections.Generic;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions.Actions.Support;

[Function("Support.RemoveObjectFromList")]
public class SupportRemoveObjectFromListFunction : VmFunction {
	public override VmType ReturnType => VmType.Boolean;
	public override int ParamCount => 2;
	public FunctionSourceParam<CommonList>? List { get; }
	public FunctionSourceParam<object>? Entity { get; }

	public SupportRemoveObjectFromListFunction(VirtualMachine vm, string[] parameters) {
		List = FunctionSourceParam<CommonList>.Read(parameters[0], vm);
		Entity = FunctionSourceParam<object>.Read(
			parameters[1], vm, CommonList.GetElementType(List, vm));
	}

	public override List<string>? GetParamStrings() => [List.Write(), Entity.Write()];
}