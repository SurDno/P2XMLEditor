using System.Xml.Linq;
using P2XMLEditor.Abstract;
using P2XMLEditor.Core;
using P2XMLEditor.Data;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.GameData.VirtualMachineElements.Enums;
using P2XMLEditor.Helper;
using static P2XMLEditor.Helper.XmlParsingHelper;

#pragma warning disable CS8618

namespace P2XMLEditor.GameData.VirtualMachineElements;

public class Event(string id) : VmElement(id) {
    protected override HashSet<string> KnownElements { get; } = [
        "EventTime", "Manual", "EventRaisingType", "EventParameter", "Condition",
        "ChangeTo", "Repeated", "Name", "Parent", "MessagesInfo"
    ];

    public TimeSpan EventTime { get; set; }
    public bool Manual { get; set; }
    public EventRaisingType EventRaisingType { get; set; }
    public bool ChangeTo { get; set; }
    public bool Repeated { get; set; }
    public string Name { get; set; }
    public VmEither<Blueprint, Quest, FunctionalComponent, Character, GameRoot> Parent { get; set; }
    public Parameter? EventParameter { get; set; }
    public Condition? Condition { get; set; }
    public List<MessageInfo>? MessagesInfo { get; set; }

    private record RawEventData(string Id, TimeSpan EventTime, bool Manual, string EventRaisingType, bool ChangeTo,
        bool Repeated, string Name, string ParentId, string? EventParameterId, string? ConditionId, 
        List<MessageInfo>? MessagesInfo) : RawData(Id);
    
    public override XElement ToXml(WriterSettings settings) {
        var element = CreateBaseElement(Id);
        if (EventParameter != null)
            element.Add(new XElement("EventParameter", EventParameter.Id));
        element.Add(
            new XElement("EventTime", $"{EventTime.Days}:{EventTime.Hours}:{EventTime.Minutes}:{EventTime.Seconds}"),
            CreateBoolElement("Manual", Manual),
            new XElement("EventRaisingType", EventRaisingType.Serialize())
        );
        if (Condition != null)
            element.Add(new XElement("Condition", Condition.Id));
        element.Add(
            CreateBoolElement("ChangeTo", ChangeTo),
            CreateBoolElement("Repeated", Repeated)
        );
        if (MessagesInfo?.Count > 0) {
            element.Add(new XElement("MessagesInfo",
                new XAttribute("count", MessagesInfo.Count),
                MessagesInfo.Select(m => new XElement("Item",
                    new XElement("Name", m.Name),
                    new XElement("Type", m.Type)
                ))
            ));
        }
        element.Add(
            new XElement("Name", Name),
            new XElement("Parent", Parent.Id)
        );
        return element;
    }

    protected override RawData CreateRawData(XElement element) {
        var messagesInfo = new List<MessageInfo>();
        var messagesElement = element.Element("MessagesInfo");
        if (messagesElement != null) {
            foreach (var item in messagesElement.Elements("Item")) {
                messagesInfo.Add(new(
                    GetRequiredElement(item, "Name").Value,
                    GetRequiredElement(item, "Type").Value
                ));
            }
        }

        return new RawEventData(
            element.Attribute("id")?.Value ?? throw new ArgumentException("Id missing"),
            ParseTimeSpan(GetRequiredElement(element, "EventTime")),
            ParseBool(GetRequiredElement(element, "Manual")),
            GetRequiredElement(element, "EventRaisingType").Value,
            ParseBool(GetRequiredElement(element, "ChangeTo")),
            ParseBool(GetRequiredElement(element, "Repeated")),
            GetRequiredElement(element, "Name").Value,
            GetRequiredElement(element, "Parent").Value,
            element.Element("EventParameter")?.Value,
            element.Element("Condition")?.Value,
            messagesInfo.Count > 0 ? messagesInfo : null
        );
    }

    public override void FillFromRawData(RawData rawData, VirtualMachine vm) {
        if (rawData is not RawEventData eventData)
            throw new ArgumentException($"Expected RawEventData but got {rawData.GetType()}");

        EventTime = eventData.EventTime;
        Manual = eventData.Manual;
        EventRaisingType = eventData.EventRaisingType.Deserialize<EventRaisingType>();
        ChangeTo = eventData.ChangeTo;
        Repeated = eventData.Repeated;
        Name = eventData.Name;
        Parent = vm.GetElement<Blueprint, Quest, FunctionalComponent, Character, GameRoot>(eventData.ParentId);
        EventParameter = eventData.EventParameterId != null ? 
            vm.GetElement<Parameter>(eventData.EventParameterId) : null;
        Condition = eventData.ConditionId != null ? 
            vm.GetElement<Condition>(eventData.ConditionId) : null;
        MessagesInfo = eventData.MessagesInfo;
    }
    
    protected override VmElement New(VirtualMachine vm, string id, VmElement parent) => throw new NotImplementedException();
}

public record struct MessageInfo(string Name, string Type);
