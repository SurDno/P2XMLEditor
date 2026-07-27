using System.Collections.Generic;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions.Actions.MapItemComponent;

[Function("MapItemComponent.AddNode")]
public class MapItemComponentAddNodeFunction : VmFunction {
	public override FunctionReturnType ReturnType => FunctionReturnType.Void;
	public override int ParamCount => 1;
	public FunctionSourceParam<MindMapNode>? Node { get; }
	public override List<string>? GetParamStrings() => [Node.Write()];
	public MapItemComponentAddNodeFunction(VirtualMachine vm, string[] parameters) {
		Node = FunctionSourceParam<MindMapNode>.Read(parameters[0], vm);
	}
}
