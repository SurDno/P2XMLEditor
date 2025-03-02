using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;
using P2XMLEditor.Helper;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions;

[Function("Support.GetListObjectsCount")]
public class GetListObjectsCountFunction : VmFunction {
	public override FunctionReturnType ReturnType => FunctionReturnType.Int;
	public override int ParamCount => 1;
   
	private readonly ParameterHolder holder;
	private readonly Parameter parameter;

	private string TEMP;
   
	public GetListObjectsCountFunction(ParameterHolder holder, Parameter parameter) {
		this.holder = holder ?? throw new ArgumentNullException(nameof(holder));
		this.parameter = parameter ?? throw new ArgumentNullException(nameof(parameter));
	}
   
	public GetListObjectsCountFunction(VirtualMachine vm, List<string> parameters) {
		if (parameters.Count != 1)
			throw new ArgumentException($"Expected 1 parameter, got {parameters.Count}");
           
		var parts = parameters[0].Split('%');
		if (parts.Length != 2)
			throw new ArgumentException($"Invalid parameter format: {parameters[0]}");
           
		holder = vm.GetElement<ParameterHolder>(parts[0]);
		parameter = ulong.TryParse(parts[1], out _) ? vm.GetElement<Parameter>(parts[1]) : null;
		if (parameter == null) TEMP = parts[1];
	}
   
	public override List<string> GetParamStrings() => [$"{holder.Id}%{parameter?.Id ?? TEMP}"];
}