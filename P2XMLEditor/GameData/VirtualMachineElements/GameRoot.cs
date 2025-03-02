using System.Xml.Linq;
using P2XMLEditor.Core;
using P2XMLEditor.Data;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.Helper;
using static P2XMLEditor.Helper.XmlParsingHelper;

namespace P2XMLEditor.GameData.VirtualMachineElements;

public class GameRoot(string id) : ParameterHolder(id) {
    private static readonly HashSet<string> BaseGameRootElements = [
        "Samples", "LogicMaps", "GameModes", "BaseToEngineGuidsTable", 
        "HierarchyScenesStructure", "HierarchyEngineGuidsTable",
        "WorldObjectSaveOptimizeMode", "FunctionalComponents", 
        "EventGraph", "ChildObjects"
    ];

    protected override HashSet<string> KnownElements => BaseGameRootElements.Concat(base.KnownElements).ToHashSet();

    public List<Sample> Samples { get; set; }
    public List<MindMap> LogicMaps { get; set; }
    public List<GameMode> GameModes { get; set; }
    public Dictionary<string, string> BaseToEngineGuidsTable { get; set; }
    public Dictionary<string, List<string>> HierarchyScenesStructure { get; set; }
    public List<string> HierarchyEngineGuidsTable { get; set; }
    public bool WorldObjectSaveOptimizeMode { get; set; }

    private record RawGameRootData(string Id, bool? Static, List<string> FunctionalComponentIds, string? EventGraphId,
        Dictionary<string, string> StandartParamIds, Dictionary<string, string> CustomParamIds, string? GameTimeContext,
        string Name, string ParentId, List<string>? InheritanceInfo, List<string>? EventIds, 
        List<string>? ChildObjectIds, List<string> SampleIds, List<string> LogicMapIds, List<string> GameModeIds,
        Dictionary<string, string> BaseToEngineGuidsTable, Dictionary<string, List<string>> HierarchyScenesStructure,
        List<string> HierarchyEngineGuidsTable, bool WorldObjectSaveOptimizeMode) : RawParameterHolderData(Id, Static, 
        FunctionalComponentIds, EventGraphId, StandartParamIds, CustomParamIds, GameTimeContext, Name, ParentId, 
        InheritanceInfo, EventIds, ChildObjectIds);

    private XElement CreateHierachyScenesStructure() {
        var structureElement = new XElement("HierarchyScenesStructure",
            new XAttribute("count", HierarchyScenesStructure.Count));
            
        foreach (var kvp in HierarchyScenesStructure) {
            var itemElement = new XElement("Item", new XAttribute("key", kvp.Key));
            if (kvp.Value.Any()) {
                itemElement.Add(new XElement("Childs",
                    new XAttribute("count", kvp.Value.Count),
                    kvp.Value.Select(id => new XElement("Item", id))));
            }
            structureElement.Add(itemElement);
        }
        
        return structureElement;
    }
    public override XElement ToXml(WriterSettings settings) {
        var element = base.ToXml(settings);
        
        // Reverse order here since we're using AddFirst.
        element.AddFirst(
            CreateListElement("Samples", Samples.Select(s => s.Id)),
            CreateListElement("LogicMaps", LogicMaps.Select(m => m.Id)),
            CreateListElement("GameModes", GameModes.Select(m => m.Id)),
            CreateDictionaryElement("BaseToEngineGuidsTable", BaseToEngineGuidsTable),
            CreateHierachyScenesStructure(),
            CreateListElement("HierarchyEngineGuidsTable", HierarchyEngineGuidsTable),
            CreateBoolElement("WorldObjectSaveOptimizeMode", WorldObjectSaveOptimizeMode)
        );

        return element;
    }

    protected override RawData CreateRawData(XElement element) {
        var baseData = (RawParameterHolderData)base.CreateRawData(element);

        var scenesStructure = new Dictionary<string, List<string>>();
        var structureElement = element.Element("HierarchyScenesStructure");
        if (structureElement != null) {
            foreach (var item in structureElement.Elements("Item")) {
                var key = item.Attribute("key")?.Value;
                if (key != null) {
                    var childsList = new List<string>();
                    var childsElement = item.Element("Childs");
                    if (childsElement != null) {
                        childsList.AddRange(childsElement.Elements("Item").Select(x => x.Value));
                    }
                    scenesStructure[key] = childsList;
                }
            }
        }

        return new RawGameRootData(
            baseData.Id,
            baseData.Static,
            baseData.FunctionalComponentIds,
            baseData.EventGraphId,
            baseData.StandartParamIds,
            baseData.CustomParamIds,
            baseData.GameTimeContext,
            baseData.Name,
            baseData.ParentId,
            baseData.InheritanceInfo,
            baseData.EventIds,
            baseData.ChildObjectIds,
            ParseListElement(element, "Samples"),
            ParseListElement(element, "LogicMaps"),
            ParseListElement(element, "GameModes"),
            ParseDictionaryElement(element, "BaseToEngineGuidsTable"),
            scenesStructure,
            ParseListElement(element, "HierarchyEngineGuidsTable"),
            ParseBool(GetRequiredElement(element, "WorldObjectSaveOptimizeMode"))
        );
    }

    public override void FillFromRawData(RawData rawData, VirtualMachine vm) {
        if (rawData is not RawGameRootData data)
            throw new ArgumentException($"Expected RawGameRootData but got {rawData.GetType()}");

        base.FillFromRawData(data, vm);

        Samples = data.SampleIds.Select(vm.GetElement<Sample>).ToList();
        LogicMaps = data.LogicMapIds.Select(vm.GetElement<MindMap>).ToList();
        GameModes = data.GameModeIds.Select(vm.GetElement<GameMode>).ToList();
        BaseToEngineGuidsTable = data.BaseToEngineGuidsTable;
        HierarchyScenesStructure = data.HierarchyScenesStructure;
        HierarchyEngineGuidsTable = data.HierarchyEngineGuidsTable;
        WorldObjectSaveOptimizeMode = data.WorldObjectSaveOptimizeMode;
    }
}