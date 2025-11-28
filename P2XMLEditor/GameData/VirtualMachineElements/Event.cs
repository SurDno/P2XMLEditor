using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using P2XMLEditor.Core;
using P2XMLEditor.Data;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.GameData.VirtualMachineElements.Enums;
using P2XMLEditor.GameData.VirtualMachineElements.Interfaces;
using P2XMLEditor.Helper;
using P2XMLEditor.Parsing.RawData;
using static P2XMLEditor.Helper.XmlParsingHelper;

#pragma warning disable CS8618

namespace P2XMLEditor.GameData.VirtualMachineElements;

public class Event(ulong id) : VmElement(id), IFiller<RawEventData> {
    protected override HashSet<string> KnownElements { get; } = [
        "EventTime", "Manual", "EventRaisingType", "EventParameter", "Condition",
        "ChangeTo", "Repeated", "Name", "Parent", "MessagesInfo"
    ];
    public TimeSpan EventTime { get; set; }
    public bool? Manual { get; set; } = true;
    public EventRaisingType EventRaisingType { get; set; }
    public bool? ChangeTo { get; set; } = true;
    public bool? Repeated { get; set; } = true;
    public string Name { get; set; }
    public VmEither<Blueprint, Quest, FunctionalComponent, Character, GameRoot> Parent { get; set; }
    public Parameter? EventParameter { get; set; }
    public Condition? Condition { get; set; }
    public List<MessageInfo>? MessagesInfo { get; set; }
    
    public override XElement ToXml(WriterSettings settings) {
        var element = CreateBaseElement(Id);
        if (EventParameter != null)
            element.Add(new XElement("EventParameter", EventParameter.Id));
        element.Add(new XElement("EventTime",
            $"{EventTime.Days}:{EventTime.Hours}:{EventTime.Minutes}:{EventTime.Seconds}"));
        
        if (Manual != null)
            element.Add(CreateBoolElement("Manual", (bool)Manual));
        element.Add(new XElement("EventRaisingType", EventRaisingType.Serialize()));
        if (Condition != null)
            element.Add(new XElement("Condition", Condition.Id));
        if (ChangeTo != null)
            element.Add(CreateBoolElement("ChangeTo", (bool)ChangeTo));
        if (Repeated != null)
            element.Add(CreateBoolElement("Repeated", (bool)Repeated));
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
    
    public void FillFromRawData(RawEventData data, VirtualMachine vm) {
        EventTime = data.EventTime;
        Manual = data.Manual;
        EventRaisingType = data.EventRaisingType;
        ChangeTo = data.ChangeTo;
        Repeated = data.Repeated;
        Name = data.Name;
        Parent = vm.GetElement<Blueprint, Quest, FunctionalComponent, Character, GameRoot>(data.ParentId);
        EventParameter = data.EventParameterId.HasValue ? 
            vm.GetElement<Parameter>(data.EventParameterId.Value) : null;
        Condition = data.ConditionId.HasValue ? 
            vm.GetElement<Condition>(data.ConditionId.Value) : null;
        MessagesInfo = data.MessagesInfo;
    }

    
    public void OnDestroy(VirtualMachine vm) {
        vm.RemoveElement(EventParameter);
        vm.RemoveElement(Condition);
        // TODO: Kill reference in parent if event is destroyed independently of parent.
    }
}

public record struct MessageInfo(string Name, string Type);
