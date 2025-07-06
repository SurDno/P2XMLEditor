using System.Xml.Linq;
using P2XMLEditor.Core;
using P2XMLEditor.Data;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.Helper;
using static P2XMLEditor.Helper.XmlParsingHelper;

#pragma warning disable CS8618

namespace P2XMLEditor.GameData.VirtualMachineElements;

public class GameString(string id) : VmElement(id) {
	protected override HashSet<string> KnownElements { get; } = ["Parent"];
	public VmEither<ParameterHolder, MindMap, Reply, Speech> Parent { get; set; }
	private Dictionary<string, string> _texts = new();
    
	public string GetText(string lang) => _texts.TryGetValue(lang, out var text) ? text : string.Empty;
	public void SetText(string text, string lang) => _texts[lang] = text;
	
	private record RawGameStringData(string Id, string ParentId) : RawData(Id);

	public override XElement ToXml(WriterSettings settings) {
		var element = CreateBaseElement(Id);
		element.Add(new XElement("Parent", Parent.Id));
		return element;
	}

	protected override RawData CreateRawData(XElement element) {
		return new RawGameStringData(
			element.Attribute("id")?.Value ?? throw new ArgumentException("Id missing"),
			GetRequiredElement(element, "Parent").Value
		);
	}

	public override void FillFromRawData(RawData rawData, VirtualMachine vm) {
		if (rawData is not RawGameStringData data)
			throw new ArgumentException($"Expected RawGameStringData but got {rawData.GetType()}");

		Parent = vm.GetElement<ParameterHolder, MindMap, Reply, Speech>(data.ParentId);
	}

	public override bool IsOrphaned() {
		return Parent.Element switch {
			MindMap mm => !mm.Nodes.Any(n => n.Content.Any(c => c.ContentDescriptionText == this)) && mm.Title != this,
			ParameterHolder ph => ph.StandartParams.All(kvp => kvp.Value.Value != Id),
			Speech s => s.Text != this,
			Reply r => r.Text != this,
			_ => true
		};
	}
	
    protected override VmElement New(VirtualMachine vm, string id, VmElement parent) => new GameString(id) {
	    Parent = new(parent),
	    _texts = vm.Languages.ToDictionary(lang => lang, _ => string.Empty)
    };
}