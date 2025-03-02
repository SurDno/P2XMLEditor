using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;
using P2XMLEditor.Helper;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions;

[Function("GameComponent.IsObjectCompatible")]
public class IsObjectCompatibleFunction : VmFunction {
    public enum ObjectComponentType {
        Common,
        Model,
        BehaviorComponent,
        Position,
        Interactive,
        Detector,
        Detectable,
        Speaking,
        LocationItem,
        Storage,
        NpcControllerComponent,
        LipSync,
        BoundCharacterComponent,
        Gate,
        MapItemComponent,
        Region,
        WaterSupplyControllerComponent,
        Id7599708409578076 // ??
    }
    
    public override FunctionReturnType ReturnType => FunctionReturnType.Bool;
    public override int ParamCount => 2;
    
    private readonly ParameterHolder holder;
    private readonly string message;    
    private readonly List<ObjectComponentType> requiredComponents;
    private readonly Parameter? constParameter;
    
    public IsObjectCompatibleFunction(ParameterHolder holder, string message, List<ObjectComponentType> requiredComponents, Parameter? constParameter = null) {
        this.holder = holder ?? throw new ArgumentNullException(nameof(holder));
        this.message = message ?? throw new ArgumentNullException(nameof(message));
        this.requiredComponents = requiredComponents ?? throw new ArgumentNullException(nameof(requiredComponents));
        this.constParameter = constParameter;
    }
    
    public IsObjectCompatibleFunction(VirtualMachine vm, List<string> parameters) {
        if (parameters.Count != 2)
            throw new ArgumentException($"Expected 2 parameters, got {parameters.Count}");
            
        var parts1 = parameters[0].Split('%');
        if (parts1.Length != 2)
            throw new ArgumentException($"Invalid first parameter format: {parameters[0]}");
            
        holder = vm.GetElement<ParameterHolder>(parts1[0]);
        message = parts1[1];
        
        var param2 = parameters[1];
        if (!param2.StartsWith("%IObjRef%cf_"))
            throw new ArgumentException($"Second parameter must start with %IObjRef%cf_: {param2}");
            
        var componentsStr = param2[12..];
        var componentNames = componentsStr.Split("&");
        
        requiredComponents = new();
        foreach (var name in componentNames) {
            if (Enum.TryParse<ObjectComponentType>(name, out var component)) {
                requiredComponents.Add(component);
            }
            else if (name == "7599708409578076") {
                requiredComponents.Add(ObjectComponentType.Id7599708409578076);
            }
            else {
                throw new ArgumentException($"Unknown component type: {name}");
            }
        }
    }
    
    public override List<string> GetParamStrings() => [
            $"{holder.Id}%{message}",
            $"%IObjRef%cf_{string.Join("&", requiredComponents.Select(c => c == ObjectComponentType.Id7599708409578076 ? "7599708409578076" : c.ToString()))}"
        ];
}