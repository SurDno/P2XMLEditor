using P2XMLEditor.GameData.Enums;
using System.Collections.Generic;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions.Actions.TagsComponent;

[Function("TagsComponent.GetTag")]
public class TagsComponentGetTagFunction(VirtualMachine vm, string[] parameters)
	: VmFunction {
	public override VmType ReturnType => VmType.Int32;
	public override int ParamCount => 1;
	public FunctionSourceParam<int> Index { get; } = FunctionSourceParam<int>.Read((parameters.Length != 0) ? parameters[0] : "", vm);
	public override List<string>? GetParamStrings() => [Index.Write()];
}