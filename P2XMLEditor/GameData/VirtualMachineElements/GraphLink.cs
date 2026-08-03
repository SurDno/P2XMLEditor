using System.Collections.Generic;
using System.Linq;
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

#pragma warning disable CS8618

namespace P2XMLEditor.GameData.VirtualMachineElements;

public class GraphLink(ulong id) : VmElement(id), IFiller<RawGraphLinkData> {
	public Event? Event { get; set; }
	public EventOwner? EventObject { get; set; }
	public int SourceExitPointIndex { get; set; }
	public int DestEntryPointIndex { get; set; }
	public List<string>? SourceParams { get; set; }
	public VmEither<Graph, Branch, Speech, State, GraphPlaceholder>? Source { get; set; }
	public VmEither<Graph, Branch, Speech, State, Talking>? Destination { get; set; }
	public bool Enabled { get; set; } = true;
	public string Name { get; set; }
	public VmEither<Graph, Talking> Parent { get; set; }

	/// <summary>
	/// An unattached link inside <paramref name="parent"/>. Both ends are left for the caller,
	/// which is what draws them; the indices start at the values the data uses for "the single
	/// unconditional exit" and "the first entry point", so a link is valid the moment its
	/// endpoints are set.
	/// </summary>
	public static GraphLink New(VirtualMachine vm, ulong id, VmElement parent) => new(id) {
		Name = "New link",
		Enabled = true,
		SourceExitPointIndex = -1,
		DestEntryPointIndex = 0,
		Parent = new(parent)
	};

	public void FillFromRawData(RawGraphLinkData data, VirtualMachine vm) {
		Event = data.EventId.HasValue ? vm.GetElement<Event>(data.EventId.Value) : null;
		EventObject = EventOwner.Read(data.EventObject, vm);
		SourceExitPointIndex = data.SourceExitPointIndex;
		DestEntryPointIndex = data.DestEntryPointIndex;
		SourceParams = data.SourceParams?.ToList();
		if (data.SourceId.HasValue)
			Source = 
			vm.GetNullableElement<Graph, Branch, Speech, State, GraphPlaceholder>(data.SourceId.Value) ?? 
			new(vm.Register(new GraphPlaceholder(data.SourceId.Value)));
		if (data.DestinationId.HasValue)
			Destination = vm.GetElement<Graph, Branch, Speech, State, Talking>(data.DestinationId.Value);
		Enabled = data.Enabled;
		Name = data.Name;
		Parent = vm.GetElement<Graph, Talking>(data.ParentId);
	}
}
