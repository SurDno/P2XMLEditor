using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using P2XMLEditor.Core;
using P2XMLEditor.Data;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.GameData.VirtualMachineElements.Enums;
using P2XMLEditor.GameData.VirtualMachineElements.Helper;
using P2XMLEditor.GameData.VirtualMachineElements.Interfaces;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes;
using P2XMLEditor.Helper;
using P2XMLEditor.Logging;
using P2XMLEditor.Parsing.RawData;
using static P2XMLEditor.Helper.XmlParsingHelper;

#pragma warning disable CS8618

namespace P2XMLEditor.GameData.VirtualMachineElements;

public class Event(ulong id) : VmElement(id), IFiller<RawEventData> {
	public TimeSpan EventTime { get; set; }
	public bool? Manual { get; set; } = true;
	public EventRaisingType EventRaisingType { get; set; }
	public bool? ChangeTo { get; set; } = true;
	public bool? Repeated { get; set; } = true;
	public string Name { get; set; }
	public VmEither<Blueprint, Quest, FunctionalComponent, Character, GameRoot> Parent { get; set; }
	public Parameter? EventParameter { get; set; }
	public Condition? Condition { get; set; }
	
	public List<Message> Messages { get; set; } = [];
	public (string Name, string Type)[]? RawMessagesInfo { get; set; } // for manual = false evts

	public IReadOnlyList<(string Name, string Type)> MessagesToWrite =>
		Manual == false ? RawMessagesInfo ?? [] : Messages.Select(m => (m.Name, m.Type)).ToArray();
	
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

		if (Manual == false) {
			RawMessagesInfo = data.MessagesInfo;
			if (Parent.Element is FunctionalComponent component)
				Messages = ApiEventTable.Get(component.Name, Name).Messages.Select(m => Message.FromApi(this, m.ParamName, m.Type)).ToList();
		} else {
			Messages = data.MessagesInfo?.Select(t => new Message(t.Item1, t.Item2, this)).ToList() ?? [];
		}
	}

	
	public override void OnDestroy(VirtualMachine vm) {
		vm.RemoveElement(EventParameter);
		vm.RemoveElement(Condition);
		// TODO: Kill reference in parent if event is destroyed independently of parent.
	}
}