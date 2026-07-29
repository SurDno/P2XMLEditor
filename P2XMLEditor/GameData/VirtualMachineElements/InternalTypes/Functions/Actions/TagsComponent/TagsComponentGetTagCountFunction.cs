using P2XMLEditor.GameData.Enums;
using System.Collections.Generic;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions.Actions.TagsComponent;

[Function("TagsComponent.GetTagCount")]
public class TagsComponentGetTagCountFunction : VmFunction {
	public override VmType ReturnType => VmType.Int32;
	public override int ParamCount => 0;
	public override List<string>? GetParamStrings() => null;
	public TagsComponentGetTagCountFunction(VirtualMachine vm, string[] parameters) {
	}
}