using System.Xml.Linq;
using P2XMLEditor.Core;
using P2XMLEditor.Data;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;

namespace P2XMLEditor.GameData.VirtualMachineElements.Placeholders;

// For cases where a part of HierarchyGuid points to a non-existing Scene.
public class ScenePlaceholder(string id) : VmElement(id) {
	protected override HashSet<string> KnownElements => throw new NotImplementedException();
	public override XElement ToXml(WriterSettings settings) => throw new NotImplementedException();
	protected override RawData CreateRawData(XElement _) => throw new NotImplementedException();
	protected override VmElement New(VirtualMachine vm, string id, VmElement parent) => throw new NotImplementedException();
	public override void FillFromRawData(RawData _, VirtualMachine __) => throw new NotImplementedException();
}