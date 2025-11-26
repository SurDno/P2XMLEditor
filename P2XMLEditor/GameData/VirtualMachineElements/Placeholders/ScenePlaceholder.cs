using System.Xml;
using System.Xml.Linq;
using P2XMLEditor.Core;
using P2XMLEditor.Data;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;

namespace P2XMLEditor.GameData.VirtualMachineElements.Placeholders;

// For cases where a part of HierarchyGuid points to a non-existing Scene.
public class ScenePlaceholder(ulong id) : VmElement(id) {
	protected override HashSet<string> KnownElements => throw new InvalidOperationException();

	public override XElement ToXml(WriterSettings settings) => throw new InvalidOperationException();
}