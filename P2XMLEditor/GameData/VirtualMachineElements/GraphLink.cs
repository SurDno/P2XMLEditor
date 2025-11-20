using System.Xml.Linq;
using P2XMLEditor.Core;
using P2XMLEditor.Data;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes;
using P2XMLEditor.GameData.VirtualMachineElements.Placeholders;
using P2XMLEditor.Helper;
using static P2XMLEditor.Helper.XmlParsingHelper;

#pragma warning disable CS8618

namespace P2XMLEditor.GameData.VirtualMachineElements;

public class GraphLink(string id) : VmElement(id) {
    protected override HashSet<string> KnownElements { get; } = [
        "Event", "EventObject", "SourceExitPointIndex", "DestEntryPointIndex",
        "SourceParams", "Source", "Destination", "Enabled", "Name", "Parent"
    ];

    public Event? Event { get; set; }
    public CommonVariable? EventObject { get; set; }
    public int SourceExitPointIndex { get; set; }
    public int DestEntryPointIndex { get; set; }
    public List<string>? SourceParams { get; set; }
    public VmEither<Graph, Branch, Speech, State, GraphPlaceholder>? Source { get; set; }
    public VmEither<Graph, Branch, Speech, State>? Destination { get; set; }
    public bool? Enabled { get; set; } = true;
    public string Name { get; set; }
    public VmEither<Graph, Talking> Parent { get; set; }

    private record RawGraphLinkData(string Id, string? Event, string EventObject, int SourceExitPointIndex,
        int DestEntryPointIndex, List<string>? SourceParams, string? Source, string? Destination, bool? Enabled, 
        string Name, string ParentId) : RawData(Id);

    public override XElement ToXml(WriterSettings settings) {
        var element = CreateBaseElement(Id);
        if (Event != null)
            element.Add(new XElement("Event", Event.Id));
        element.Add(
            new XElement("EventObject", EventObject.Write()),
            new XElement("SourceExitPointIndex", SourceExitPointIndex),
            new XElement("DestEntryPointIndex", DestEntryPointIndex)
        );
        if (SourceParams?.Count > 0)
            element.Add(CreateListElement("SourceParams", SourceParams));
        if (Source != null)
            element.Add(new XElement("Source", Source.Id));
        if (Destination != null)
            element.Add(new XElement("Destination", Destination.Id));
        if (Enabled != null)
            element.Add(CreateBoolElement("Enabled", (bool)Enabled));
        element.Add(
            new XElement("Name", Name),
            new XElement("Parent", Parent.Id)
        );
        return element;
    }

    protected override RawData CreateRawData(XElement element) {
        return new RawGraphLinkData(
            element.Attribute("id")?.Value ?? throw new ArgumentException("Id missing"),
            element.Element("Event")?.Value,
            GetRequiredElement(element, "EventObject").Value,
            GetRequiredElement(element, "SourceExitPointIndex").ParseInt(),
            GetRequiredElement(element, "DestEntryPointIndex").ParseInt(),
            ParseListElement(element, "SourceParams"),
            element.Element("Source")?.Value,
            element.Element("Destination")?.Value,
            element.Element("Enabled")?.Let(ParseBool),
            GetRequiredElement(element, "Name").Value,
            GetRequiredElement(element, "Parent").Value
        );
    }

    public override void FillFromRawData(RawData rawData, VirtualMachine vm) {
        if (rawData is not RawGraphLinkData data)
            throw new ArgumentException($"Expected RawGraphLinkData but got {rawData.GetType()}");

        Event = data.Event != null ? vm.GetElement<Event>(data.Event) : null;
        EventObject = CommonVariable.Read(data.EventObject, vm);
        SourceExitPointIndex = data.SourceExitPointIndex;
        DestEntryPointIndex = data.DestEntryPointIndex;
        SourceParams = data.SourceParams;
        Source = data.Source != null ? 
            vm.GetNullableElement<Graph, Branch, Speech, State, GraphPlaceholder>(data.Source) ?? 
            new(vm.Register(new GraphPlaceholder(data.Source))) : null;
        Destination = data.Destination != null ? vm.GetElement<Graph, Branch, Speech, State>(data.Destination) : null;
        Enabled = data.Enabled;
        Name = data.Name;
        Parent = vm.GetElement<Graph, Talking>(data.ParentId);
    }
    
    protected override VmElement New(VirtualMachine vm, string id, VmElement parent) => throw new NotImplementedException();
}