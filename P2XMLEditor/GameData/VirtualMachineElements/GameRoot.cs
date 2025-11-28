using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using P2XMLEditor.Core;
using P2XMLEditor.Data;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.GameData.VirtualMachineElements.Interfaces;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes;
using P2XMLEditor.Helper;
using P2XMLEditor.Parsing.RawData;
using static P2XMLEditor.Helper.XmlParsingHelper;

namespace P2XMLEditor.GameData.VirtualMachineElements;

public class GameRoot(ulong id) : ParameterHolder(id), IFiller<RawGameRootData> {
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
    public Dictionary<ulong, SceneStructureEntry> HierarchyScenesStructure { get; set; }
    public List<string> HierarchyEngineGuidsTable { get; set; }
    public bool? WorldObjectSaveOptimizeMode { get; set; }

    private XElement CreateHierachyScenesStructure() {
        var structureElement = new XElement("HierarchyScenesStructure",
            new XAttribute("count", HierarchyScenesStructure.Count));

        foreach (var (key, entry) in HierarchyScenesStructure) {
            var itemElement = new XElement("Item", new XAttribute("key", key));

            foreach (var (type, children) in entry.Containers) {
                if (children.Count == 0) continue;
                var container = new XElement(type.Serialize(),
                    new XAttribute("count", children.Count),
                    children.Select(id => new XElement("Item", id.ToString()))
                );
                itemElement.Add(container);
            }

            structureElement.Add(itemElement);
        }

        return structureElement;
    }

    public void FillFromRawData(RawGameRootData data, VirtualMachine vm) {
        FunctionalComponents = data.FunctionalComponentIds.Select(vm.GetElement<FunctionalComponent>).ToList();
        EventGraph = data.EventGraphId.HasValue ? vm.GetElement<Graph>(data.EventGraphId.Value) : null;
        StandartParams = data.StandartParamIds.ToDictionary(kv => kv.Key, kv => vm.GetElement<Parameter>(kv.Value));
        CustomParams = data.CustomParamIds.ToDictionary(kv => kv.Key, kv => vm.GetElement<Parameter>(kv.Value));
        Name = data.Name;
        Parent = null;
        Events = data.EventIds?.Select(vm.GetElement<Event>).ToList();
        ChildObjects = data.ChildObjectIds?.Select(vm.GetElement<ParameterHolder>).ToList();
        Samples = data.SampleIds.Select(vm.GetElement<Sample>).ToList();
        LogicMaps = data.LogicMapIds.Select(vm.GetElement<MindMap>).ToList();
        GameModes = data.GameModeIds.Select(vm.GetElement<GameMode>).ToList();
        BaseToEngineGuidsTable = data.BaseToEngineGuidsTable;
        HierarchyScenesStructure = data.HierarchyScenesStructure;
        HierarchyEngineGuidsTable = data.HierarchyEngineGuidsTable;
        WorldObjectSaveOptimizeMode = data.WorldObjectSaveOptimizeMode;
    }
    
    public override XElement ToXml(WriterSettings settings) {
        var element = base.ToXml(settings);
        
        // Reverse order here since we're using AddFirst.
        if (WorldObjectSaveOptimizeMode != null)
            element.AddFirst(CreateBoolElement("WorldObjectSaveOptimizeMode", (bool)WorldObjectSaveOptimizeMode));
        element.AddFirst(
            CreateListElement("Samples", Samples.Select(s => s.Id.ToString())),
            CreateListElement("LogicMaps", LogicMaps.Select(m => m.Id.ToString())),
            CreateListElement("GameModes", GameModes.Select(m => m.Id.ToString())),
            CreateDictionaryElement("BaseToEngineGuidsTable", BaseToEngineGuidsTable),
            CreateHierachyScenesStructure(),
            CreateListElement("HierarchyEngineGuidsTable", HierarchyEngineGuidsTable)
        );

        return element;
    }
}