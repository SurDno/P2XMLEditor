using System.Xml.Linq;
using P2XMLEditor.Abstract;
using P2XMLEditor.Core;
using P2XMLEditor.Data;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.Helper;
using static P2XMLEditor.Helper.XmlParsingHelper;

#pragma warning disable CS8618

namespace P2XMLEditor.GameData.VirtualMachineElements;

public class EntryPoint(string id) : VmElement(id) {
	protected override HashSet<string> KnownElements { get; } = ["Name", "ActionLine", "Parent"];

	public string Name { get; set; }
	public ActionLine? ActionLine { get; set; }
	public VmEither<State, Graph, Branch, Speech, Talking> Parent { get; set; }

	private record RawEntryPointData(string Id, string Name, string? ActionLineId, string ParentId) : RawData(Id);
	
	public override XElement ToXml(WriterSettings settings) {
		var element = CreateBaseElement(Id);
		element.Add(new XElement("Name", Name));
		if (ActionLine != null)
			element.Add(new XElement("ActionLine", ActionLine.Id));
		element.Add(new XElement("Parent", Parent.Id));
		return element;
	}

	protected override RawData CreateRawData(XElement element) {
		return new RawEntryPointData(
			element.Attribute("id")?.Value ?? throw new ArgumentException("Id missing"),
			GetRequiredElement(element, "Name").Value,
			element.Element("ActionLine")?.Value,
			GetRequiredElement(element, "Parent").Value
		);
	}

	public override void FillFromRawData(RawData rawData, VirtualMachine vm) {
		if (rawData is not RawEntryPointData data)
			throw new ArgumentException($"Expected RawEntryPointData but got {rawData.GetType()}");

		Name = data.Name;
		ActionLine = data.ActionLineId != null ? vm.GetElement<ActionLine>(data.ActionLineId) : null;
		Parent = vm.GetElement<State, Graph, Branch, Speech, Talking>(data.ParentId);
	}
	
	protected override VmElement New(VirtualMachine vm, string id, VmElement parent) => throw new NotImplementedException();
	
	public override void OnDestroy(VirtualMachine vm) {
		if (ActionLine != null)
			vm.RemoveElement(ActionLine);
	}
}