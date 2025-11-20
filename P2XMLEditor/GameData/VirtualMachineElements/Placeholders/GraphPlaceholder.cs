using System.Xml.Linq;
using P2XMLEditor.Core;
using P2XMLEditor.Data;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;

namespace P2XMLEditor.GameData.VirtualMachineElements.Placeholders;

// For cases where Source property in GraphLink point to a non-existing Graph.
public class GraphPlaceholder(string id) : VmElement(id) {
	protected override HashSet<string> KnownElements => throw new InvalidOperationException();
	public override XElement ToXml(WriterSettings settings) => throw new InvalidOperationException();
	protected override RawData CreateRawData(XElement _) => throw new InvalidOperationException();
	protected override VmElement New(VirtualMachine vm, string id, VmElement parent) => throw new InvalidOperationException();

	public override void FillFromRawData(RawData _, VirtualMachine __) => throw new InvalidOperationException();
}