using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;
using P2XMLEditor.Helper;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions;

[Function("GameComponent.GetObjectClass")]
public class GetObjectClassFunction : VmFunction {
	public override FunctionReturnType ReturnType => FunctionReturnType.BlueprintRef;
	public override int ParamCount => 1;
   
	private readonly ParameterHolder holder;
	private readonly string? messageParam; 
   
	public GetObjectClassFunction(ParameterHolder holder, string? messageParam = null) {
		this.holder = holder ?? throw new ArgumentNullException(nameof(holder));
		this.messageParam = messageParam;
	}
   
	public GetObjectClassFunction(VirtualMachine vm, List<string> parameters) {
		if (parameters.Count != 1)
			throw new ArgumentException($"Expected 1 parameter, got {parameters.Count}");
           
		var parts = parameters[0].Split('%', 2);
		holder = vm.GetElement<ParameterHolder>(parts[0]);
		messageParam = parts.Length > 1 ? parts[1] : null;
	}
   
	public override List<string> GetParamStrings() =>
		[messageParam != null ? $"{holder.Id}%{messageParam}" : holder.Id];
}