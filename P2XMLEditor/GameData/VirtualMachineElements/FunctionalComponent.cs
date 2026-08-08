using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using P2XMLEditor.Core;
using P2XMLEditor.Data;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.GameData.VirtualMachineElements.Interfaces;
using P2XMLEditor.Helper;
using P2XMLEditor.Parsing.RawData;
using static P2XMLEditor.Helper.XmlParsingHelper;

#pragma warning disable CS8618

namespace P2XMLEditor.GameData.VirtualMachineElements;

public class FunctionalComponent(ulong id) : VmElement(id), IFiller<RawFunctionalComponentData> {
	public List<Event> Events { get; set; }
	public bool Main { get; set; }
	public long LoadPriority { get; set; }
	public string Name { get; set; }
	public ParameterHolder Parent { get; set; }


	public void FillFromRawData(RawFunctionalComponentData data, VirtualMachine vm) {
		Events = [];
		if (data.EventIds != null) {
			foreach (var eventId in data.EventIds)
				Events.Add(vm.GetElement<Event>(eventId));
		}
		Main = data.Main;
		LoadPriority = data.LoadPriority;
		Name = data.Name;
		Parent = vm.GetElement<ParameterHolder>(data.ParentId);
	}

	/// <summary>
	/// A component with no events and no parameters yet. The parameters it declares are added by
	/// <see cref="Helper.ComponentCatalogue"/>, which knows what the component looks like on the
	/// objects that already carry it; a component alone has no way to know.
	/// </summary>
	public static FunctionalComponent New(VirtualMachine vm, ulong id, VmElement parent) => new(id) {
		Name = "NewComponent",
		Main = false,
		LoadPriority = 0,
		Events = [],
		Parent = (parent as ParameterHolder)!
	};

	public override void OnDestroy(VirtualMachine vm) {
		var compToRemove = Parent.FunctionalComponents.FirstOrDefault(f => f == this);
		if (compToRemove != null)
			Parent.FunctionalComponents.Remove(compToRemove);
		foreach (var @event in Events.ToList())
			vm.RemoveElement(@event);
	}
}

