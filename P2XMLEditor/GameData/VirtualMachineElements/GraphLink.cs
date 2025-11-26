using System.Xml;
using System.Xml.Linq;
using P2XMLEditor.Core;
using P2XMLEditor.Data;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.GameData.VirtualMachineElements.Interfaces;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes;
using P2XMLEditor.GameData.VirtualMachineElements.Placeholders;
using P2XMLEditor.Helper;
using P2XMLEditor.Parsing.RawData;
using static P2XMLEditor.Helper.XmlParsingHelper;
using static P2XMLEditor.Helper.XmlReaderExtensions;

#pragma warning disable CS8618

namespace P2XMLEditor.GameData.VirtualMachineElements;

public class GraphLink(ulong id) : VmElement(id), IFiller<RawGraphLinkData> {
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
    public VmEither<Graph, Branch, Speech, State, Talking>? Destination { get; set; }
    public bool? Enabled { get; set; } = true;
    public string Name { get; set; }
    public VmEither<Graph, Talking> Parent { get; set; }

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
    
    public void FillFromRawData(RawGraphLinkData data, VirtualMachine vm) {
        Event = data.EventId.HasValue ? vm.GetElement<Event>(data.EventId.Value) : null;
        EventObject = CommonVariable.Read(data.EventObject, vm);
        SourceExitPointIndex = data.SourceExitPointIndex;
        DestEntryPointIndex = data.DestEntryPointIndex;
        SourceParams = data.SourceParams;
        Source = data.SourceId.HasValue ? 
            vm.GetNullableElement<Graph, Branch, Speech, State, GraphPlaceholder>(data.SourceId.Value) ?? 
            new(vm.Register(new GraphPlaceholder(data.SourceId.Value))) : null;
        Destination = data.DestinationId.HasValue ? 
            vm.GetElement<Graph, Branch, Speech, State, Talking>(data.DestinationId.Value) : null;
        Enabled = data.Enabled;
        Name = data.Name;
        Parent = vm.GetElement<Graph, Talking>(data.ParentId);
    }
}