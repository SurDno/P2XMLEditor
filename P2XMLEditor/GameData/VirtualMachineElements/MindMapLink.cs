using System.Xml;
using System.Xml.Linq;
using P2XMLEditor.Core;
using P2XMLEditor.Data;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.GameData.VirtualMachineElements.Interfaces;
using P2XMLEditor.Helper;
using P2XMLEditor.Parsing.RawData;
using static P2XMLEditor.Helper.XmlParsingHelper;
using static P2XMLEditor.Helper.XmlReaderExtensions;

namespace P2XMLEditor.GameData.VirtualMachineElements;

public class MindMapLink(ulong id) : VmElement(id), IFiller<RawMindMapLinkData>, IVmCreator<MindMapLink> {
	protected override HashSet<string> KnownElements { get; } = ["Parent", "Source", "Destination", "Name"];

	public MindMap Parent { get; set; }
	public MindMapNode Source { get; set; }
	public MindMapNode Destination { get; set; }


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
	
	public void FillFromRawData(RawMindMapLinkData data, VirtualMachine vm) {
		Parent = vm.GetElement<MindMap>(data.ParentId);
		Source = vm.GetElement<MindMapNode>(data.SourceId);
		Destination = vm.GetElement<MindMapNode>(data.DestinationId);
	}

	public static MindMapLink New(VirtualMachine vm, ulong id, VmElement parent) {
		var node = new MindMapLink(id) {
			Parent = parent as MindMap ?? throw new ArgumentException("MindMapLink parent must be MindMap")
		};
		node.Parent.Links.Add(node);
		return node;
	}

	public void OnDestroy(VirtualMachine vm) {
		Parent.Links.Remove(this);
		Source.OutputLinks.Remove(this);
		Destination.InputLinks.Remove(this);
	}
}
