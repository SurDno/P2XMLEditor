using System.Xml.Linq;
using P2XMLEditor.Core;
using P2XMLEditor.Data;
using static P2XMLEditor.Helper.XmlParsingHelper;

#pragma warning disable CS8618

namespace P2XMLEditor.GameData.VirtualMachineElements.Abstract;

public abstract class GameObject(string id) : ParameterHolder(id) {
    private static readonly HashSet<string> BaseGameObjectElements =
        ["WorldPositionGuid", "EngineTemplateID", "EngineBaseTemplateID", "Instantiated"];
    
    protected override HashSet<string> KnownElements => BaseGameObjectElements.Concat(base.KnownElements).ToHashSet();

    public string? WorldPositionGuid { get; set; }
    public string? EngineTemplateId { get; set; }
    public string? EngineBaseTemplateId { get; set; }
    public bool? Instantiated { get; set; }

    private record RawGameObjectData(string Id, bool? Static, List<string> FunctionalComponentIds, string? EventGraphId,
        Dictionary<string, string> StandartParamIds, Dictionary<string, string> CustomParamIds, string? GameTimeContext,
        string Name, string? ParentId, List<string>? InheritanceInfo, List<string>? EventIds, 
        List<string>? ChildObjectIds, string? WorldPositionGuid, string? EngineTemplateId, string? EngineBaseTemplateId,
        bool? Instantiated) : RawParameterHolderData(Id, Static, FunctionalComponentIds, EventGraphId, StandartParamIds, 
        CustomParamIds, GameTimeContext, Name, ParentId, InheritanceInfo, EventIds, ChildObjectIds);

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

    protected override RawData CreateRawData(XElement element) {
        var baseData = (RawParameterHolderData)base.CreateRawData(element);

        if (baseData.ParentId == null)
            throw new ArgumentException($"Parent is for {GetType().Name}");

        return new RawGameObjectData(
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
            element.Element("WorldPositionGuid")?.Value,
            element.Element("EngineTemplateID")?.Value,
            element.Element("EngineBaseTemplateID")?.Value,      
            element.Element("Instantiated")?.Let(ParseBool)
        );
    }

    public override void FillFromRawData(RawData rawData, VirtualMachine vm) {
        if (rawData is not RawGameObjectData data)
            throw new ArgumentException($"Expected RawGameObjectData but got {rawData.GetType()}");

        base.FillFromRawData(data, vm);
        
        WorldPositionGuid = data.WorldPositionGuid;
        EngineTemplateId = data.EngineTemplateId;
        EngineBaseTemplateId = data.EngineBaseTemplateId;
        Instantiated = data.Instantiated;
    }
}