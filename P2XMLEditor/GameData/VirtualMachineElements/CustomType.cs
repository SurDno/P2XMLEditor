using System.Xml.Linq;
using P2XMLEditor.Abstract;
using P2XMLEditor.Core;
using P2XMLEditor.Data;
using P2XMLEditor.Helper;
using static P2XMLEditor.Helper.XmlParsingHelper;

#pragma warning disable CS8618

namespace P2XMLEditor.GameData.VirtualMachineElements;

public class CustomType(string id) : VmElement(id) {
	protected override HashSet<string> KnownElements { get; } = ["Name", "Parent"];
    
	public string Name { get; set; }
	public GameRoot Parent { get; set; }

	private record RawCustomTypeData(string Id, string Name, string ParentId) : RawData(Id);

	public override XElement ToXml(WriterSettings settings) {
		var element = CreateBaseElement(Id);
		element.Add(
			new XElement("Name", Name),
			new XElement("Parent", Parent?.Id ?? string.Empty)
		);
		return element;
	}

	protected override RawData CreateRawData(XElement element) {
		return new RawCustomTypeData(
			element.Attribute("id")?.Value ?? throw new ArgumentException("Id missing"),
			GetRequiredElement(element, "Name").Value,
			GetRequiredElement(element, "Parent").Value
		);
	}

	protected override VmElement New(VirtualMachine vm, string id, VmElement parent) => 
		throw new NotImplementedException();

	public override void FillFromRawData(RawData rawData, VirtualMachine vm) {
		if (rawData is not RawCustomTypeData data)
			throw new ArgumentException($"Expected RawCustomTypeData but got {rawData.GetType()}");

		Name = data.Name;
		Parent = vm.GetElement<GameRoot>(data.ParentId);
	}
}