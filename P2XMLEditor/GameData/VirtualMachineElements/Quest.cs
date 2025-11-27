using System.Xml;
using System.Xml.Linq;
using P2XMLEditor.Core;
using P2XMLEditor.Data;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.GameData.VirtualMachineElements.Interfaces;
using P2XMLEditor.Helper;
using P2XMLEditor.Parsing.RawData;

using static P2XMLEditor.Helper.XmlParsingHelper;
using static P2XMLEditor.Helper.XmlReaderExtensions;

namespace P2XMLEditor.GameData.VirtualMachineElements;

public class Quest(ulong id) : ParameterHolder(id), IFiller<RawQuestData> {
    private static readonly HashSet<string> BaseQuestElements = ["StartEvent"];
    protected override HashSet<string> KnownElements => BaseQuestElements.Concat(base.KnownElements).ToHashSet();
    
    public Event? StartEvent { get; set; }
    
    public void FillFromRawData(RawQuestData data, VirtualMachine vm) {
        Static = data.Static;
        FunctionalComponents = data.FunctionalComponentIds.Select(vm.GetElement<FunctionalComponent>).ToList();
        EventGraph = vm.GetElement<Graph>(data.EventGraphId);
        StandartParams = data.StandartParamIds.ToDictionary(kvp => kvp.Key, kvp => vm.GetElement<Parameter>(kvp.Value));
        CustomParams = data.CustomParamIds.ToDictionary(kvp => kvp.Key, kvp => vm.GetElement<Parameter>(kvp.Value));
        GameTimeContext = data.GameTimeContext;
        Name = data.Name;
        Parent = vm.GetElement<ParameterHolder>(data.ParentId);
        InheritanceInfo = data.InheritanceInfo ?? [];
        Events = data.EventIds != null ? data.EventIds.Select(vm.GetElement<Event>).ToList() : [];
        ChildObjects = data.ChildObjectIds != null 
            ? data.ChildObjectIds.Select(vm.GetElement<ParameterHolder>).ToList() : [];

        StartEvent = data.StartEventId.HasValue ? vm.GetElement<Event>(data.StartEventId.Value) : null;
    }
    
    public override XElement ToXml(WriterSettings settings) {
        var element = base.ToXml(settings);
        if (StartEvent != null)
            element.AddFirst(new XElement("StartEvent", StartEvent.Id));
        return element;
    }

}