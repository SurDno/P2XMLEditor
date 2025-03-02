using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions;

[Function("Storage.GetItemsCount")]
public class GetItemsCountFunction : VmFunction {
	public override FunctionReturnType ReturnType => FunctionReturnType.Int;
	public override int ParamCount => 0;
   
	public GetItemsCountFunction() {}
   
	public GetItemsCountFunction(VirtualMachine vm, List<string> parameters) {
		if (parameters.Count != 0)
			throw new ArgumentException($"Expected no parameters, got {parameters.Count}");
	}
   
	public override List<string>? GetParamStrings() => null;
}