using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.GameData.VirtualMachineElements.Interfaces;
using P2XMLEditor.Helper;
using P2XMLEditor.Parsing.RawData;

namespace P2XMLEditor.GameData.VirtualMachineElements;

public class Blueprint(ulong id) : ParameterHolder(id), IFiller<RawBlueprintData> {
	public void FillFromRawData(RawBlueprintData data, VirtualMachine vm) {
		Static = data.Static;
		FunctionalComponents = data.FunctionalComponentIds.Select(vm.GetElement<FunctionalComponent>).ToList();
		EventGraph = data.EventGraphId.HasValue ? vm.GetElement<Graph>(data.EventGraphId.Value) : null;
		StandartParams = data.StandartParamIds.ToDictionary(kv => kv.Key, kv => vm.GetElement<Parameter>(kv.Value));
		CustomParams = data.CustomParamIds.ToDictionary(kv => kv.Key, kv => vm.GetElement<Parameter>(kv.Value));
		GameTimeContext = data.GameTimeContext;
		Name = data.Name;
		Parent = data.ParentId.HasValue ? vm.GetElement<ParameterHolder>(data.ParentId.Value) : null;
		InheritanceInfo = data.InheritanceInfo;
		Events = data.EventIds?.Select(vm.GetElement<Event>).ToList();
		ChildObjects = data.ChildObjectIds?.Select(vm.GetElement<ParameterHolder>).ToList();
	}
}