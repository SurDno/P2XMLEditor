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

public class CustomType(ulong id) : VmElement(id), IFiller<RawCustomTypeData> {
	protected override HashSet<string> KnownElements { get; } = ["Name", "Parent"];
    
	public string Name { get; set; }
	public GameRoot Parent { get; set; }

	public override XElement ToXml(WriterSettings settings) {
		Console.WriteLine(Id);
		var element = CreateBaseElement(Id);
		element.Add(
			new XElement("Name", Name),
			new XElement("Parent", Parent.Id.ToString())
		);
		return element;
	}

	public void FillFromRawData(RawCustomTypeData data, VirtualMachine vm) {
		Console.WriteLine(Id);
		Name = data.Name;
		Parent = vm.GetElement<GameRoot>(data.ParentId);
	}
	
	public static VmElement New(VirtualMachine vm, ulong id, VmElement parent) => 
		throw new NotImplementedException();
}