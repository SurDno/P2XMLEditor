using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions;

// Used to preserve self-closing tags of TargetFunctionName in expressions.
[Function("")]
public class FunctionPlaceholder : VmFunction {
	public override FunctionReturnType ReturnType => FunctionReturnType.Void;
	public override int ParamCount => 0;
	public override List<string>? GetParamStrings() => null;
	
	public FunctionPlaceholder(VirtualMachine vm, List<string> parameters) { }
}