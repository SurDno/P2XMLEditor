using System.Xml.Linq;
using P2XMLEditor.Core;
using P2XMLEditor.Data;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.Helper;

namespace P2XMLEditor.GameData.VirtualMachineElements;

public class Quest(string id) : ParameterHolder(id) {
    private static readonly HashSet<string> BaseQuestElements = ["StartEvent"];
    protected override HashSet<string> KnownElements => BaseQuestElements.Concat(base.KnownElements).ToHashSet();
    
    public Event? StartEvent { get; set; }

    private record RawQuestData(string Id, bool? Static, List<string> FunctionalComponentIds, string? EventGraphId,
        Dictionary<string, string> StandartParamIds, Dictionary<string, string> CustomParamIds, string? GameTimeContext,
        string Name, string ParentId, List<string>? InheritanceInfo, List<string>? EventIds, 
        List<string>? ChildObjectIds, string? StartEventId) : RawParameterHolderData(Id, Static, FunctionalComponentIds, 
        EventGraphId, StandartParamIds, CustomParamIds, GameTimeContext, Name, ParentId, InheritanceInfo, EventIds, 
        ChildObjectIds);

    protected override RawData CreateRawData(XElement element) {
        var baseData = (RawParameterHolderData)base.CreateRawData(element);
        
        return new RawQuestData(
            baseData.Id, baseData.Static, baseData.FunctionalComponentIds, baseData.EventGraphId,
            baseData.StandartParamIds, baseData.CustomParamIds, baseData.GameTimeContext,
            baseData.Name, baseData.ParentId, baseData.InheritanceInfo, baseData.EventIds,
            baseData.ChildObjectIds, element.Element("StartEvent")?.Value
        );
    }

    public override XElement ToXml(WriterSettings settings) {
        var element = base.ToXml(settings);
        if (StartEvent != null)
            element.AddFirst(new XElement("StartEvent", StartEvent.Id));
        return element;
    }

    public override void FillFromRawData(RawData rawData, VirtualMachine vm) {
        if (rawData is not RawQuestData data)
            throw new ArgumentException($"Expected RawQuestData but got {rawData.GetType()}");

        base.FillFromRawData(data, vm);
        StartEvent = data.StartEventId != null ?
            vm.GetElement<Event>(data.StartEventId) : null;
    }
}