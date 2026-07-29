using P2XMLEditor.GameData.Enums;
using System.Collections.Generic;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions.Actions.Support;

[Function("Support.AddObjectToList")]
public class SupportAddObjectToListFunction : VmFunction {
	public override VmType ReturnType => VmType.Void;
	public override int ParamCount => 2;
	public FunctionSourceParam<CommonList>? ListParam { get; }
	public FunctionSourceParam<object>? ObjectParam { get; }

	public SupportAddObjectToListFunction(VirtualMachine vm, string[] parameters) {
		ListParam = FunctionSourceParam<CommonList>.Read(parameters[0], vm);
		// Engine signature is AddObjectToList(VMCommonList, object) â€” element type comes from the list.
		ObjectParam = FunctionSourceParam<object>.Read(
			parameters[1], vm, CommonList.GetElementType(ListParam, vm));
	}

	public override List<string>? GetParamStrings() => [ListParam.Write(), ObjectParam.Write()];
}