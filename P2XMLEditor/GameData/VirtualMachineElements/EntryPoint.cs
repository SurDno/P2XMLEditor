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

public class EntryPoint(ulong id) : VmElement(id), IFiller<RawEntryPointData> {
	protected override HashSet<string> KnownElements { get; } = ["Name", "ActionLine", "Parent"];

	public string Name { get; set; }
	public ActionLine? ActionLine { get; set; }
	public VmEither<State, Graph, Branch, Speech, Talking> Parent { get; set; }

	public override XElement ToXml(WriterSettings settings) {
		var element = CreateBaseElement(Id);
		element.Add(new XElement("Name", Name));
		if (ActionLine != null)
			element.Add(new XElement("ActionLine", ActionLine.Id));
		element.Add(new XElement("Parent", Parent.Id));
		return element;
	}

	public void FillFromRawData(RawEntryPointData data, VirtualMachine vm) {
		Name = data.Name;
		ActionLine = data.ActionLineId.HasValue ? 
			vm.GetElement<ActionLine>(data.ActionLineId.Value) : null;
		Parent = vm.GetElement<State, Graph, Branch, Speech, Talking>(data.ParentId);
	}
	
	public static VmElement New(VirtualMachine vm, ulong id, VmElement parent) => throw new NotImplementedException();
	
	public void OnDestroy(VirtualMachine vm) {
		if (ActionLine != null)
			vm.RemoveElement(ActionLine);
	}
}