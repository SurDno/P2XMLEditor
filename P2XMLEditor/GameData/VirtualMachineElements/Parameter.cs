using System.Xml.Linq;
using P2XMLEditor.Core;
using P2XMLEditor.Data;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.GameData.VirtualMachineElements.Interfaces;
using P2XMLEditor.Helper;
using static P2XMLEditor.Helper.XmlParsingHelper;

#pragma warning disable CS8618

namespace P2XMLEditor.GameData.VirtualMachineElements;

public class Parameter(string id) : VmElement(id), ICommonVariableParameter {
    protected override HashSet<string> KnownElements { get; } =
        ["Name", "OwnerComponent", "Type", "Value", "Implicit", "Parent", "Custom"];

    public string Name { get; set; }
    public FunctionalComponent? OwnerComponent { get; set; }
    public string Type { get; set; }
    public string Value { get; set; }
    public bool? Implicit { get; set; }
    public VmEither<ParameterHolder, Expression> Parent { get; set; }
    public bool? Custom { get; set; }

    private record RawParameterData(string Id, string Name, string? OwnerComponentId, string Type, string Value,
        bool? Implicit, string ParentId, bool? Custom) : RawData(Id);

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

    protected override RawData CreateRawData(XElement element) {
        return new RawParameterData(
            element.Attribute("id")?.Value ?? throw new ArgumentException("Id missing"),
            GetRequiredElement(element, "Name").Value,
            element.Element("OwnerComponent")?.Value,
            GetRequiredElement(element, "Type").Value,
            GetRequiredElement(element, "Value").Value,
            element.Element("Implicit")?.Let(ParseBool),
            GetRequiredElement(element, "Parent").Value,
            element.Element("Custom")?.Let(ParseBool)
        );
    }

    public override void FillFromRawData(RawData rawData, VirtualMachine vm) {
        if (rawData is not RawParameterData data)
            throw new ArgumentException($"Expected RawParameterData but got {rawData.GetType()}");

        Name = data.Name;
        OwnerComponent = data.OwnerComponentId != null ?
            vm.GetElement<FunctionalComponent>(data.OwnerComponentId) : null;
        Type = data.Type;
        Value = data.Value;
        Implicit = data.Implicit;
        Parent = vm.GetElement<ParameterHolder, Expression>(data.ParentId);
        Custom = data.Custom;
    }
    
    public override bool IsOrphaned() {
        return Parent.Element switch {
            ParameterHolder ph => ph.StandartParams.Concat(ph.CustomParams).All(p => p.Value != this),
            Expression e => e.Const != this,
            _ => true
        };
    }
    
    protected override VmElement New(VirtualMachine vm, string id, VmElement parent) {
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

    public override void OnDestroy(VirtualMachine vm) {
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
    }
}