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

#pragma warning disable CS8618

namespace P2XMLEditor.GameData.VirtualMachineElements;

public class GameString(ulong id) : VmElement(id), IFiller<RawGameStringData>, IVmCreator<GameString> {
	protected override HashSet<string> KnownElements { get; } = ["Parent"];
	public VmEither<ParameterHolder, MindMap, Reply, Speech> Parent { get; set; }
	private Dictionary<string, string> _texts = new();
    
	public string GetText(string lang) => _texts.TryGetValue(lang, out var text) ? text : string.Empty;
	public void SetText(string text, string lang) => _texts[lang] = text;

	public override XElement ToXml(WriterSettings settings) {
		var element = CreateBaseElement(Id);
		element.Add(new XElement("Parent", Parent.Id));
		return element;
	}

	public override bool IsOrphaned() {
		return Parent.Element switch {
			MindMap mm => !mm.Nodes.Any(n => n.Content.Any(c => c.ContentDescriptionText == this)) && mm.Title != this,
			ParameterHolder ph => ph.StandartParams.All(kvp => kvp.Value.Value != Id.ToString()),
			Speech s => s.Text != this,
			Reply r => r.Text != this,
			_ => true
		};
	}
	
	public void FillFromRawData(RawGameStringData data, VirtualMachine vm) {
		Parent = vm.GetElement<ParameterHolder, MindMap, Reply, Speech>(data.ParentId);
	}
	
    public static GameString New(VirtualMachine vm, ulong id, VmElement parent) => new GameString(id) {
	    Parent = new(parent),
	    _texts = vm.Languages.ToDictionary(lang => lang, _ => string.Empty)
    };
}