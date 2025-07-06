using System.Xml.Linq;
using P2XMLEditor.Core;
using P2XMLEditor.Data;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.GameData.VirtualMachineElements.Enums;
using P2XMLEditor.Helper;
using static P2XMLEditor.Helper.XmlParsingHelper;

#pragma warning disable CS8618

namespace P2XMLEditor.GameData.VirtualMachineElements;

public class Condition(string id) : VmElement(id) {
	protected override HashSet<string> KnownElements { get; } = ["Predicates", "Operation", "Name", "OrderIndex"];

	public List<VmEither<Condition, PartCondition>> Predicates { get; set; }
	public ConditionOperation Operation { get; set; }
	public string Name { get; set; }
	public byte OrderIndex { get; set; }

	private record RawConditionData(string Id, List<string> PredicateIds, string Operation, string Name,
		int OrderIndex) : RawData(Id);

	public override XElement ToXml(WriterSettings settings) {
		var element = CreateBaseElement(Id);
		if (Predicates.Any())
			element.Add(CreateListElement("Predicates", Predicates.Select(p => p.Id)));
		element.Add(
			new XElement("Operation", Operation.Serialize()),
			CreateSelfClosingElement("Name", Name),
			new XElement("OrderIndex", OrderIndex)
		);
		return element;
	}

	protected override RawData CreateRawData(XElement element) {
		return new RawConditionData(
			element.Attribute("id")?.Value ?? throw new ArgumentException("Id missing"),
			ParseListElement(element, "Predicates"),
			GetRequiredElement(element, "Operation").Value,
			GetRequiredElement(element, "Name").Value,
			ParseInt(GetRequiredElement(element, "OrderIndex"))
		);
	}

	public override void FillFromRawData(RawData rawData, VirtualMachine vm) {
		if (rawData is not RawConditionData data)
			throw new ArgumentException($"Expected RawConditionData but got {rawData.GetType()}");

		Predicates = data.PredicateIds.Select(vm.GetElement<Condition, PartCondition>).ToList();
		Operation = data.Operation.Deserialize<ConditionOperation>();
		Name = data.Name;
		OrderIndex = (byte)data.OrderIndex;
	}
	
	protected override VmElement New(VirtualMachine vm, string id, VmElement parent) => new Condition(id) {
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