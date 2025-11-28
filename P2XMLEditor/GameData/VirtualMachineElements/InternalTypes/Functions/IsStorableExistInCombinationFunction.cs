using System;
using System.Collections.Generic;
using System.Linq;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;
using P2XMLEditor.Helper;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Functions;

[Function("GlobalStorageManager.IsStorableExistInCombibation")]
public class IsStorableExistInCombinationFunction : VmFunction {
    public override FunctionReturnType ReturnType => FunctionReturnType.Bool;
    public override int ParamCount => 2;
    
    private readonly GameRoot root;
    private readonly Item combination;
    private readonly Parameter? parameter;    
    private readonly Item? templateItem;  
    private readonly bool isSecondParameter;   
    
    public IsStorableExistInCombinationFunction(VirtualMachine vm, GameRoot root, Item combination, Parameter parameter) {
        this.root = root ?? throw new ArgumentNullException(nameof(root));
        this.combination = combination ?? throw new ArgumentNullException(nameof(combination));
        this.parameter = parameter ?? throw new ArgumentNullException(nameof(parameter));
        isSecondParameter = true;
    }
    
    public IsStorableExistInCombinationFunction(VirtualMachine vm, GameRoot root, Item combination, Item templateItem) {
        this.root = root ?? throw new ArgumentNullException(nameof(root));
        this.combination = combination ?? throw new ArgumentNullException(nameof(combination));
        this.templateItem = templateItem ?? throw new ArgumentNullException(nameof(templateItem));
        isSecondParameter = false;
    }
    
    public IsStorableExistInCombinationFunction(VirtualMachine vm, List<string> parameters) {
        if (parameters.Count != 2)
            throw new ArgumentException($"Expected 2 parameters, got {parameters.Count}");
            
        var parts1 = parameters[0].Split('%');
        if (parts1.Length != 2)
            throw new ArgumentException($"Invalid first parameter format: {parameters[0]}");
            
        root = vm.GetElement<GameRoot>(ulong.Parse(parts1[0]));
        combination = vm.GetElement<Item>(ulong.Parse(parts1[1]));
        
        var parts2 = parameters[1].Split('%');
        if (parts2.Length != 2)
            throw new ArgumentException($"Invalid second parameter format: {parameters[1]}");
        
        try {
            parameter = vm.GetElement<Parameter>(ulong.Parse(parts2[1]));
            isSecondParameter = true;
        }
        catch {
            templateItem = vm.GetElementsByType<Item>().FirstOrDefault(item => item.EngineTemplateId == parts2[1])
                ?? throw new ArgumentException($"No item found with template ID {parts2[1]}");
            isSecondParameter = false;
        }
    }
    
    public override List<string> GetParamStrings() => [
        $"{root.Id}%{combination.Id}",
        isSecondParameter ? $"{parameter!.Parent!.Id}%{parameter.Id}" : $"{root.Id}%{templateItem!.EngineTemplateId}"
    ];
}