using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using P2XMLEditor.Core;
using P2XMLEditor.Data;
using P2XMLEditor.GameData.VirtualMachineElements.Interfaces;
using P2XMLEditor.Helper;
using P2XMLEditor.Parsing.RawData;
using static P2XMLEditor.Helper.XmlParsingHelper;

#pragma warning disable CS8618

namespace P2XMLEditor.GameData.VirtualMachineElements.Abstract;

public abstract class GameObject(ulong id) : ParameterHolder(id), IFiller<RawGameObjectData> {
	private static readonly HashSet<string> BaseGameObjectElements =
		["WorldPositionGuid", "EngineTemplateID", "EngineBaseTemplateID", "Instantiated"];

	public string? WorldPositionGuid { get; set; }
	public string? EngineTemplateId { get; set; }
	public string? EngineBaseTemplateId { get; set; }
	public bool? Instantiated { get; set; }

	
	public void FillFromRawData(RawGameObjectData data, VirtualMachine vm) {
		WorldPositionGuid = data.WorldPositionGuid;
		EngineTemplateId = data.EngineTemplateId;
		EngineBaseTemplateId = data.EngineBaseTemplateId;
		Instantiated = data.Instantiated;
		Static = data.Static;
		FunctionalComponents = [];
		if (data.FunctionalComponentIds != null) {
			foreach (var functionalComponentId in data.FunctionalComponentIds)
				FunctionalComponents.Add(vm.GetElement<FunctionalComponent>(functionalComponentId));
		}
		EventGraph = data.EventGraphId.HasValue ? vm.GetElement<Graph>(data.EventGraphId.Value) : null;
		StandartParams = new();
		if (data.StandartParamIds != null) {
			foreach (var kvp in data.StandartParamIds)
				StandartParams.Add(kvp.Item1, vm.GetElement<Parameter>(kvp.Item2));
		}
		CustomParams = new();
		if (data.CustomParamIds != null) {
			foreach (var kvp in data.CustomParamIds)
				CustomParams.Add(kvp.Item1, vm.GetElement<Parameter>(kvp.Item2));
		}
		GameTimeContext = data.GameTimeContext;
		Name = data.Name;
		Parent = vm.GetElement<ParameterHolder>(data.ParentId);
		InheritanceInfo = data.InheritanceInfo?.ToList();
		Events = [];
		if (data.EventIds != null) {
			foreach (var eventId in data.EventIds)
				Events.Add(vm.GetElement<Event>(eventId));
		}
		ChildObjects = [];
		if (data.ChildObjectIds != null) {
			foreach (var eventId in data.ChildObjectIds)
				ChildObjects.Add(vm.GetElement<ParameterHolder>(eventId));
		}
		WorldPositionGuid = data.WorldPositionGuid;
		EngineTemplateId = data.EngineTemplateId;
		EngineBaseTemplateId = data.EngineBaseTemplateId;
		Instantiated = data.Instantiated;
	}
}