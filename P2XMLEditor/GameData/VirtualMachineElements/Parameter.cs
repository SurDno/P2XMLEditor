using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using P2XMLEditor.Core;
using P2XMLEditor.Data;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.GameData.VirtualMachineElements.Interfaces;
using P2XMLEditor.Helper;
using P2XMLEditor.Parsing.RawData;
using static P2XMLEditor.Helper.XmlParsingHelper;

#pragma warning disable CS8618

namespace P2XMLEditor.GameData.VirtualMachineElements;

public class Parameter(ulong id) : VmElement(id), IFiller<RawParameterData>, ICommonVariableParameter, IVmCreator<Parameter> {
    protected override HashSet<string> KnownElements { get; } =
        ["Name", "OwnerComponent", "Type", "Value", "Implicit", "Parent", "Custom"];
    public string Name { get; set; }
    public FunctionalComponent? OwnerComponent { get; set; }
    public string Type { get; set; }
    public string Value { get; set; }
    public bool? Implicit { get; set; }
    public VmEither<ParameterHolder, Expression> Parent { get; set; }
    public bool? Custom { get; set; }

    public override XElement ToXml(WriterSettings settings) {
        var element = CreateBaseElement(Id);
        element.Add(
            CreateSelfClosingElement("Name", Name)
        );
        if (OwnerComponent != null)
            element.Add(new XElement("OwnerComponent", OwnerComponent.Id));
        element.Add(
            new XElement("Type", Type),
            CreateSelfClosingElement("Value", Value)
        );
        if (Implicit != null)
            element.Add(CreateBoolElement("Implicit", (bool)Implicit));
        element.Add(new XElement("Parent", Parent.Id));
        if (Custom != null)
            element.Add(CreateBoolElement("Custom", (bool)Custom));
        return element;
    }

    public override bool IsOrphaned() {
        return Parent.Element switch {
            ParameterHolder ph => ph.StandartParams.Concat(ph.CustomParams).All(p => p.Value != this),
            Expression e => e.Const != this,
            _ => true
        };
    }
    
    public void FillFromRawData(RawParameterData data, VirtualMachine vm) {
        Name = data.Name;
        OwnerComponent = data.OwnerComponentId.HasValue
            ? vm.GetElement<FunctionalComponent>(data.OwnerComponentId.Value)
            : null;
        Type = data.Type;
        Value = data.Value;
        Implicit = data.Implicit;
        Parent = vm.GetElement<ParameterHolder, Expression>(data.ParentId);
        Custom = data.Custom;
    }

    public static Parameter New(VirtualMachine vm, ulong id, VmElement parent) {
        var par = new Parameter(id) {
            Name = "NewParam",
            Parent = new(parent),
            Implicit = false,
            Custom = false,
            Type = "System.Boolean",
            Value = "False"
        };
        return par;
    }

    public void OnDestroy(VirtualMachine vm) {
        switch (Parent.Element) {
            case ParameterHolder ph:          
                var keyToRemove = ph.StandartParams.FirstOrDefault(kvp => kvp.Value == this).Key;
                if (keyToRemove != null)
                    ph.StandartParams.Remove(keyToRemove);
                keyToRemove = ph.CustomParams.FirstOrDefault(kvp => kvp.Value == this).Key;
                if (keyToRemove != null)
                    ph.CustomParams.Remove(keyToRemove);
                break;
            case Expression e:
                // Do we need to redo something about the expression if we remove the const value? Needs testing.
                e.Const = null;
                break;
        }
        
        // TODO: Redo when we start parsing types like normal human beings.
        if (Type == "ITextRef" && !string.IsNullOrEmpty(Value))
            vm.RemoveElement(vm.GetElement<GameString>(ulong.Parse(Value)));
    }

    public string ParamId => id.ToString();
}