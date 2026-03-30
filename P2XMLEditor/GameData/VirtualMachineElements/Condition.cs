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

public class Condition(ulong id) : VmElement(id), IFiller<RawConditionData>, IVmCreator<Condition> {
	public List<VmEither<Condition, PartCondition>> Predicates { get; set; }
	public ConditionOperation Operation { get; set; }
	public string Name { get; set; }
	public byte OrderIndex { get; set; }

	
	public void FillFromRawData(RawConditionData data, VirtualMachine vm) {
		Predicates = [];
		foreach (var id in data.PredicateIds)
			Predicates.Add(vm.GetElement<Condition, PartCondition>(id));
		Operation = data.Operation;
		Name = data.Name;
		OrderIndex = (byte)data.OrderIndex;
	}
	
	public static Condition New(VirtualMachine vm, ulong id, VmElement parent) => new Condition(id) {
		Name = "Default Condition",
		Operation = ConditionOperation.Root,
		OrderIndex = 0,
		Predicates = [new VmEither<Condition, PartCondition>(CreateDefault<PartCondition>(vm, null!))]
	};

	public override void OnDestroy(VirtualMachine vm) {
		foreach (var predicate in Predicates)
			vm.RemoveElement(predicate.Element);
	}
}