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

namespace P2XMLEditor.GameData.VirtualMachineElements;

public class GameRoot(ulong id) : ParameterHolder(id), IFiller<RawGameRootData> {
	private static readonly HashSet<string> BaseGameRootElements = [
		"Samples", "LogicMaps", "GameModes", "BaseToEngineGuidsTable", 
		"HierarchyScenesStructure", "HierarchyEngineGuidsTable",
		"WorldObjectSaveOptimizeMode", "FunctionalComponents", 
		"EventGraph", "ChildObjects"
	];

	public List<Sample> Samples { get; set; }
	public List<MindMap> LogicMaps { get; set; }
	public List<GameMode> GameModes { get; set; }
	public Dictionary<string, string> BaseToEngineGuidsTable { get; set; }
	public Dictionary<ulong, Dictionary<ChildContainerType, ulong[]>> HierarchyScenesStructure { get; set; }
	public List<string> HierarchyEngineGuidsTable { get; set; }
	public bool WorldObjectSaveOptimizeMode { get; set; }


	public void FillFromRawData(RawGameRootData data, VirtualMachine vm) {
		FunctionalComponents = data.FunctionalComponentIds.Select(vm.GetElement<FunctionalComponent>).ToList();
		EventGraph = data.EventGraphId.HasValue ? vm.GetElement<Graph>(data.EventGraphId.Value) : null;
		StandartParams = data.StandartParamIds.ToDictionary(kv => kv.Item1, kv => vm.GetElement<Parameter>(kv.Item2));
		CustomParams = data.CustomParamIds.ToDictionary(kv => kv.Item1, kv => vm.GetElement<Parameter>(kv.Item2));
		Name = data.Name;
		Parent = null;
		Events = data.EventIds?.Select(vm.GetElement<Event>).ToList();
		ChildObjects = data.ChildObjectIds?.Select(vm.GetElement<ParameterHolder>).ToList();
		Samples = data.SampleIds.Select(vm.GetElement<Sample>).ToList();
		LogicMaps = data.LogicMapIds.Select(vm.GetElement<MindMap>).ToList();
		GameModes = data.GameModeIds.Select(vm.GetElement<GameMode>).ToList();
		BaseToEngineGuidsTable = data.BaseToEngineGuidsTable;
		HierarchyScenesStructure = data.HierarchyScenesStructure;
		HierarchyEngineGuidsTable = data.HierarchyEngineGuidsTable.ToList();
		WorldObjectSaveOptimizeMode = data.WorldObjectSaveOptimizeMode;
	}
	
}
