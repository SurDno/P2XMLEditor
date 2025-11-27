using System.Xml;
using System.Xml.Linq;
using P2XMLEditor.Core;
using P2XMLEditor.Data;
using P2XMLEditor.GameData.VirtualMachineElements.Interfaces;
using P2XMLEditor.Helper;
using P2XMLEditor.Parsing.RawData;

using static P2XMLEditor.Helper.XmlParsingHelper;
using static P2XMLEditor.Helper.XmlReaderExtensions;

#pragma warning disable CS8618

namespace P2XMLEditor.GameData.VirtualMachineElements.Abstract;

public abstract class GameObject(ulong id) : ParameterHolder(id), IFiller<RawGameObjectData> {
    private static readonly HashSet<string> BaseGameObjectElements =
        ["WorldPositionGuid", "EngineTemplateID", "EngineBaseTemplateID", "Instantiated"];
    
    protected override HashSet<string> KnownElements => BaseGameObjectElements.Concat(base.KnownElements).ToHashSet();

    public string? WorldPositionGuid { get; set; }
    public string? EngineTemplateId { get; set; }
    public string? EngineBaseTemplateId { get; set; }
    public bool? Instantiated { get; set; }

    public override XElement ToXml(WriterSettings settings) {
        var element = base.ToXml(settings);
        
        // Reverse order here since we're using AddFirst.
        if (Instantiated != null)
           element.AddFirst(CreateBoolElement("Instantiated", (bool)Instantiated));
        if (EngineBaseTemplateId != null)
            element.AddFirst(CreateSelfClosingElement("EngineBaseTemplateID", EngineBaseTemplateId));
        if (EngineTemplateId != null)
            element.AddFirst(CreateSelfClosingElement("EngineTemplateID", EngineTemplateId));
        if (WorldPositionGuid != null)
            element.AddFirst(CreateSelfClosingElement("WorldPositionGuid", WorldPositionGuid));
        return element;
    }
    
    public void FillFromRawData(RawGameObjectData data, VirtualMachine vm) {
        Static = data.Static;
        FunctionalComponents = data.FunctionalComponentIds?.Select(vm.GetElement<FunctionalComponent>).ToList() ?? [];
        EventGraph = data.EventGraphId.HasValue ? vm.GetElement<Graph>(data.EventGraphId.Value) : null;
        StandartParams = data.StandartParamIds?.ToDictionary(kv => kv.Key, kv => vm.GetElement<Parameter>(kv.Value)) ?? [];
        CustomParams = data.CustomParamIds?.ToDictionary(kv => kv.Key, kv => vm.GetElement<Parameter>(kv.Value)) ?? [];
        GameTimeContext = data.GameTimeContext;
        Name = data.Name;
        Parent = vm.GetElement<ParameterHolder>(data.ParentId);
        InheritanceInfo = data.InheritanceInfo;
        Events = data.EventIds?.Select(vm.GetElement<Event>).ToList();
        ChildObjects = data.ChildObjectIds?.Select(vm.GetElement<ParameterHolder>).ToList();
        WorldPositionGuid = data.WorldPositionGuid;
        EngineTemplateId = data.EngineTemplateId;
        EngineBaseTemplateId = data.EngineBaseTemplateId;
        Instantiated = data.Instantiated;
    }
}