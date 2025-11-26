using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;
using P2XMLEditor.Helper;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions;

[Function("Trigger.IsContainsObject")]
public class IsContainsObjectFunction : VmFunction {
	public override FunctionReturnType ReturnType => FunctionReturnType.Entity;
	public override int ParamCount => 1;
   
	private readonly GameObject gameObject;
   
	public IsContainsObjectFunction(GameObject gameObject) {
		this.gameObject = gameObject ?? throw new ArgumentNullException(nameof(gameObject));
	}
   
	public IsContainsObjectFunction(VirtualMachine vm, List<string> parameters) {
		if (parameters.Count != 1)
			throw new ArgumentException($"Expected 1 parameter, got {parameters.Count}");
           
		var parts = parameters[0].Split('%');
		if (parts.Length != 2)
			throw new ArgumentException($"Invalid parameter format: {parameters[0]}");
           
		if (parts[0] != parts[1])
			throw new ArgumentException($"Both parts of parameter must be the same GameObject ID: {parameters[0]}");
           
		gameObject = vm.GetElement<GameObject>(ulong.Parse(parts[0]));
	}
   
	public override List<string> GetParamStrings() => [$"{gameObject.Id}%{gameObject.Id}"];
}