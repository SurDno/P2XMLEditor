using System.Xml;
using System.Xml.Linq;
using P2XMLEditor.Core;
using P2XMLEditor.Data;
using P2XMLEditor.GameData.VirtualMachineElements.Interfaces;
using P2XMLEditor.Helper;
using static P2XMLEditor.Helper.XmlParsingHelper;
using static P2XMLEditor.Helper.XmlReaderExtensions;

#pragma warning disable CS8618

namespace P2XMLEditor.GameData.VirtualMachineElements.Abstract;

public abstract class ParameterHolder(ulong id) : VmElement(id), ICommonVariableParameter {
    protected override HashSet<string> KnownElements { get; } = [
        "Static", "FunctionalComponents", "EventGraph", "StandartParams", "CustomParams",
        "GameTimeContext", "Name", "Parent", "InheritanceInfo", "Events", "ChildObjects"
    ];
    public bool? Static { get; set; }
    public List<FunctionalComponent> FunctionalComponents { get; set; }
    public Graph? EventGraph { get; set; }
    public Dictionary<string, Parameter> StandartParams { get; set; }
    public Dictionary<string, Parameter>? CustomParams { get; set; }
    public string? GameTimeContext { get; set; }
    public string Name { get; set; }
    public ParameterHolder? Parent { get; set; }
    public List<string>? InheritanceInfo { get; set; }
    public List<Event>? Events { get; set; }
    public List<ParameterHolder>? ChildObjects { get; set; }

    public override XElement ToXml(WriterSettings settings) {
        var element = CreateBaseElement(id);
        if (Static != null)
            element.Add(CreateBoolElement("Static", (bool)Static));
        if (InheritanceInfo?.Any() == true)
            element.Add(CreateListElement("InheritanceInfo", InheritanceInfo));
        if (FunctionalComponents.Any())
            element.Add(CreateListElement("FunctionalComponents", FunctionalComponents.Select(f => f.Id.ToString())));
        if (EventGraph != null)
            element.Add(new XElement("EventGraph", EventGraph.Id));
        if (ChildObjects?.Any() == true)
            element.Add(CreateListElement("ChildObjects", ChildObjects.Select(c => c.Id.ToString())));
        if (Events?.Any() == true)
            element.Add(CreateListElement("Events", Events.Select(e => e.Id.ToString())));
        if (CustomParams.Any())
            element.Add(CreateDictionaryElement("CustomParams", CustomParams.ToDictionary(kv => kv.Key, kv => kv.Value.Id.ToString())));
        if (StandartParams.Any())
            element.Add(CreateDictionaryElement("StandartParams", StandartParams.ToDictionary(kv => kv.Key, kv => kv.Value.Id.ToString())));
        if (GameTimeContext != null)
            element.Add(new XElement("GameTimeContext", GameTimeContext));
        element.Add(new XElement("Name", Name));
        
        if (Parent != null)
            element.Add(new XElement("Parent", Parent.Id));
        else if (this is not GameRoot)
            throw new InvalidOperationException($"Parent is missing for {GetType().Name} {ParamId}");
        
        return element;
    }


    public void OnDestroy(VirtualMachine vm) {
        foreach (var functionalComponent in FunctionalComponents.ToList())
            vm.RemoveElement(functionalComponent);
        if (EventGraph != null)
            vm.RemoveElement(EventGraph);
        foreach (var kvp in StandartParams.ToList()) 
            vm.RemoveElement(kvp.Value);
        foreach (var kvp in CustomParams.ToList()) 
            vm.RemoveElement(kvp.Value);
        foreach (var ev in Events?.ToList() ?? [])
            vm.RemoveElement(ev);
        foreach (var ph in vm.GetElementsByType<ParameterHolder>()) {
            if (ph.ChildObjects != null && ph.ChildObjects.Contains(this))
                ph.ChildObjects.Remove(this);
        }
        
        // There are cases of one-sided ParameterHolder-GameString relations.
        // Those are all outdated strings and aren't actually used by the game, but will crash P2XMLE on later reloads.
        // So they have to be forcibly removed at this step (but can theoretically be cleaned up earlier).
        foreach (var gs in vm.GetElementsByType<GameString>().Where(g => g.Parent.Element == this).ToList())
            vm.RemoveElement(gs);
    }

    public string ParamId => id.ToString();
}