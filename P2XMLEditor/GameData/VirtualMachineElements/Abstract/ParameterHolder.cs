using System.Xml.Linq;
using P2XMLEditor.Abstract;
using P2XMLEditor.Core;
using P2XMLEditor.Data;
using P2XMLEditor.GameData.VirtualMachineElements.Interfaces;
using P2XMLEditor.Helper;
using static P2XMLEditor.Helper.XmlParsingHelper;

#pragma warning disable CS8618

namespace P2XMLEditor.GameData.VirtualMachineElements.Abstract;

public abstract class ParameterHolder(string id) : VmElement(id), ICommonVariableParameter {
    protected override HashSet<string> KnownElements { get; } = [
        "Static", "FunctionalComponents", "EventGraph", "StandartParams", "CustomParams",
        "GameTimeContext", "Name", "Parent", "InheritanceInfo", "Events", "ChildObjects"
    ];

    public bool? Static { get; set; }
    public List<FunctionalComponent> FunctionalComponents { get; set; }
    public Graph? EventGraph { get; set; }
    public Dictionary<string, Parameter> StandartParams { get; set; }
    public Dictionary<string, Parameter> CustomParams { get; set; }
    public string? GameTimeContext { get; set; }
    public string Name { get; set; }
    public ParameterHolder? Parent { get; set; }
    public List<string>? InheritanceInfo { get; set; }
    public List<Event>? Events { get; set; }
    public List<ParameterHolder>? ChildObjects { get; set; }

    protected record RawParameterHolderData(string Id, bool? Static, List<string> FunctionalComponentIds, 
        string? EventGraphId, Dictionary<string, string> StandartParamIds, Dictionary<string, string> CustomParamIds,
        string? GameTimeContext, string Name, string? ParentId, List<string>? InheritanceInfo, List<string>? EventIds,
        List<string>? ChildObjectIds) : RawData(Id);

    public override XElement ToXml(WriterSettings settings) {
        var element = CreateBaseElement(Id);
        if (Static != null)
            element.Add(CreateBoolElement("Static", (bool)Static));
        if (InheritanceInfo?.Any() == true)
            element.Add(CreateListElement("InheritanceInfo", InheritanceInfo));
        if (FunctionalComponents.Any())
            element.Add(CreateListElement("FunctionalComponents", FunctionalComponents.Select(f => f.Id)));
        if (EventGraph != null)
            element.Add(new XElement("EventGraph", EventGraph.Id));
        if (ChildObjects?.Any() == true)
            element.Add(CreateListElement("ChildObjects", ChildObjects.Select(c => c.Id)));
        if (Events?.Any() == true)
            element.Add(CreateListElement("Events", Events.Select(e => e.Id)));
        if (CustomParams.Any())
            element.Add(CreateDictionaryElement("CustomParams", CustomParams.ToDictionary(kv => kv.Key, kv => kv.Value.Id)));
        if (StandartParams.Any())
            element.Add(CreateDictionaryElement("StandartParams", StandartParams.ToDictionary(kv => kv.Key, kv => kv.Value.Id)));
        if (GameTimeContext != null)
            element.Add(new XElement("GameTimeContext", GameTimeContext));
        element.Add(new XElement("Name", Name));
        
        if (Parent != null)
            element.Add(new XElement("Parent", Parent.Id));
        else if (this is not GameRoot)
            throw new InvalidOperationException($"Parent is missing for {GetType().Name} {Id}");
        
        return element;
    }

    protected override RawData CreateRawData(XElement element) {
        var parentElement = element.Element("Parent");
        var parentId = parentElement?.Value;
        
        if (parentId == null && this is not GameRoot)
            throw new InvalidOperationException($"Parent is missing for {GetType().Name} {Id}");

        return new RawParameterHolderData(
            element.Attribute("id")?.Value ?? throw new ArgumentException("Id missing"),
            element.Element("Static")?.Let(ParseBool),
            ParseListElement(element, "FunctionalComponents"),
            element.Element("EventGraph")?.Value,
            ParseDictionaryElement(element, "StandartParams"),
            ParseDictionaryElement(element, "CustomParams"),
            element.Element("GameTimeContext")?.Value,
            GetRequiredElement(element, "Name").Value,
            parentId,
            ParseListElement(element, "InheritanceInfo"),
            ParseListElement(element, "Events"),
            ParseListElement(element, "ChildObjects")
        );
    }
    
    public override void FillFromRawData(RawData rawData, VirtualMachine vm) {
        if (rawData is not RawParameterHolderData data)
            throw new ArgumentException($"Expected RawParameterHolderData but got {rawData.GetType()}");

        Static = data.Static;
        FunctionalComponents = data.FunctionalComponentIds.Select(vm.GetElement<FunctionalComponent>).ToList();
        EventGraph = data.EventGraphId != null ? vm.GetElement<Graph>(data.EventGraphId) : null;
        StandartParams = data.StandartParamIds.ToDictionary(kv => kv.Key, kv => vm.GetElement<Parameter>(kv.Value));
        CustomParams = data.CustomParamIds.ToDictionary(kv => kv.Key, kv => vm.GetElement<Parameter>(kv.Value));
        GameTimeContext = data.GameTimeContext;
        Name = data.Name;
        Parent = data.ParentId != null ? vm.GetElement<ParameterHolder>(data.ParentId) : null;
        InheritanceInfo = data.InheritanceInfo;
        Events = data.EventIds?.Select(vm.GetElement<Event>).ToList();
        ChildObjects = data.ChildObjectIds?.Select(vm.GetElement<ParameterHolder>).ToList();
    }
    
    protected override VmElement New(VirtualMachine vm, string id, VmElement parent) => throw new NotImplementedException();

    public override void OnDestroy(VirtualMachine vm) {
        foreach(var functionalComponent in FunctionalComponents)
            vm.RemoveElement(functionalComponent);
        if (EventGraph != null)
            vm.RemoveElement(EventGraph);
        foreach(var kvp in StandartParams)
            vm.RemoveElement(kvp.Value);
        foreach(var kvp in CustomParams)
            vm.RemoveElement(kvp.Value);
        foreach(var ev in Events)
            vm.RemoveElement(ev);
    }
}