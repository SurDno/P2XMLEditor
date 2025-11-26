using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;
using P2XMLEditor.Helper;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions;

[Function("Support.CheckListObjectExist")]
public class CheckListObjectExistFunction : VmFunction {
    public override FunctionReturnType ReturnType => FunctionReturnType.Bool;
    public override int ParamCount => 2;
    
    private readonly ParameterHolder holder, holder2;
    private readonly Parameter firstParameter;
    private readonly Parameter? secondAsParameter;
    private readonly string? secondAsMessage;
    private readonly bool isSecondMessage;
    
    public CheckListObjectExistFunction(ParameterHolder holder, Parameter firstParameter, Parameter secondParameter) {
        this.holder = holder ?? throw new ArgumentNullException(nameof(holder));
        this.firstParameter = firstParameter ?? throw new ArgumentNullException(nameof(firstParameter));
        secondAsParameter = secondParameter ?? throw new ArgumentNullException(nameof(secondParameter));
        isSecondMessage = false;
    }
    
    public CheckListObjectExistFunction(ParameterHolder holder, Parameter firstParameter, string message) {
        this.holder = holder ?? throw new ArgumentNullException(nameof(holder));
        this.firstParameter = firstParameter ?? throw new ArgumentNullException(nameof(firstParameter));
        secondAsMessage = message ?? throw new ArgumentNullException(nameof(message));
        isSecondMessage = true;
    }
    
    public CheckListObjectExistFunction(VirtualMachine vm, List<string> parameters) {
        if (parameters.Count != 2)
            throw new ArgumentException($"Expected 2 parameters, got {parameters.Count}");
            
        var parts1 = parameters[0].Split('%');
        if (parts1.Length != 2)
            throw new ArgumentException($"Invalid first parameter format: {parameters[0]}");
            
        holder = vm.GetElement<ParameterHolder>(ulong.Parse(parts1[0]));
        firstParameter = vm.GetElement<Parameter>(ulong.Parse(parts1[1]));
        
        var parts2 = parameters[1].Split('%');
        if (parts2.Length != 2)
            throw new ArgumentException($"Invalid second parameter format: {parameters[1]}");
            
        holder2 = vm.GetElement<ParameterHolder>(ulong.Parse(parts2[0]));
        
        try {
            secondAsParameter = vm.GetElement<Parameter>(ulong.Parse(parts2[1]));
            isSecondMessage = false;
        } catch {
            secondAsMessage = parts2[1];
            isSecondMessage = true;
        }
    }
    
    public override List<string> GetParamStrings() => [
        $"{holder.Id}%{firstParameter.Id}",
        $"{holder2.Id}%{(isSecondMessage ? secondAsMessage : secondAsParameter!.Id)}"
    ];
}