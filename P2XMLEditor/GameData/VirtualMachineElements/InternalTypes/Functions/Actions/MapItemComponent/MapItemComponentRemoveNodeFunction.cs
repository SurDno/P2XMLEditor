using P2XMLEditor.GameData.Enums;
using System.Collections.Generic;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions.Actions.MapItemComponent;

[Function("MapItemComponent.RemoveNode")]
public class MapItemComponentRemoveNodeFunction : VmFunction {
	public override VmType ReturnType => VmType.Void;
	public override int ParamCount => 1;
	public FunctionSourceParam<MindMapNode>? Node { get; }
	public override List<string>? GetParamStrings() => [Node.Write()];
	public MapItemComponentRemoveNodeFunction(VirtualMachine vm, string[] parameters) {
		Node = FunctionSourceParam<MindMapNode>.Read(parameters[0], vm);
	}
}