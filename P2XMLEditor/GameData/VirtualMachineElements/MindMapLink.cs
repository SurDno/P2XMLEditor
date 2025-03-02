using System.Xml.Linq;
using P2XMLEditor.Abstract;
using P2XMLEditor.Core;
using P2XMLEditor.Data;
using P2XMLEditor.Helper;
using static P2XMLEditor.Helper.XmlParsingHelper;

namespace P2XMLEditor.GameData.VirtualMachineElements;

public class MindMapLink(string id) : VmElement(id) {
	protected override HashSet<string> KnownElements { get; } = ["Parent", "Source", "Destination", "Name"];

	public MindMap Parent { get; set; }
	public MindMapNode Source { get; set; }
	public MindMapNode Destination { get; set; }

	private record RawMindMapLinkData(string Id, string ParentId, string SourceId, string DestinationId) : RawData(Id);

	public override XElement ToXml(WriterSettings settings) {
		var element = CreateBaseElement(Id);
		element.Add(
			new XElement("Source", Source.Id),
			new XElement("Destination", Destination.Id),
			new XElement("Name"),
			new XElement("Parent", Parent.Id)
		);
		return element;
	}

	protected override RawData CreateRawData(XElement element) {
		return new RawMindMapLinkData(
			element.Attribute("id")?.Value ?? throw new ArgumentException("Id missing"),
			GetRequiredElement(element, "Parent").Value,
			GetRequiredElement(element, "Source").Value,
			GetRequiredElement(element, "Destination").Value
		);
	}

	public override void FillFromRawData(RawData rawData, VirtualMachine vm) {
		if (rawData is not RawMindMapLinkData data)
			throw new ArgumentException($"Expected RawMindMapLinkData but got {rawData.GetType()}");

		Parent = vm.GetElement<MindMap>(data.ParentId);
		Source = vm.GetElement<MindMapNode>(data.SourceId);
		Destination = vm.GetElement<MindMapNode>(data.DestinationId);
	}

	protected override VmElement New(VirtualMachine vm, string id, VmElement parent) {
		var node = new MindMapLink(id) {
			Parent = parent as MindMap ?? throw new ArgumentException("MindMapLink parent must be MindMap")
		};
		node.Parent.Links.Add(node);
		return node;
	}

	public override void OnDestroy(VirtualMachine vm) {
		Parent.Links.Remove(this);
		Source.OutputLinks.Remove(this);
		Destination.InputLinks.Remove(this);
	}
}
