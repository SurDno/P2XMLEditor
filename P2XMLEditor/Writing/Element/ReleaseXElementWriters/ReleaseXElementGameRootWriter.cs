using System.Linq;
using System.Xml.Linq;
using P2XMLEditor.Data;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.Helper;
using static P2XMLEditor.Helper.XmlParsingHelper;

namespace P2XMLEditor.Writing.Element.ReleaseXElementWriters;

public class ReleaseXElementGameRootWriter : ReleaseXElementParameterHolderWriter<GameRoot> {
	public override XElement ToXml(GameRoot element, WriterSettings settings) {
		var xElement = base.ToXml(element, settings);
		
		// Reverse order here since we're using AddFirst.
		if (!settings.RemoveDefaultValueTypes || element.WorldObjectSaveOptimizeMode)
			xElement.AddFirst(CreateBoolElement("WorldObjectSaveOptimizeMode", element.WorldObjectSaveOptimizeMode));
		xElement.AddFirst(
			CreateListElement("Samples", element.Samples.Select(s => s.Id.ToString())),
			CreateListElement("LogicMaps", element.LogicMaps.Select(m => m.Id.ToString())),
			CreateListElement("GameModes", element.GameModes.Select(m => m.Id.ToString())),
			CreateDictionaryElement("BaseToEngineGuidsTable", element.BaseToEngineGuidsTable),
			CreateHierachyScenesStructure(element),
			CreateListElement("HierarchyEngineGuidsTable", element.HierarchyEngineGuidsTable)
		);

		return EnsureFullClosingTag(xElement);
	}

	private XElement CreateHierachyScenesStructure(GameRoot element) {
		var structureElement = new XElement("HierarchyScenesStructure",
			new XAttribute("count", element.HierarchyScenesStructure.Count));

		foreach (var (key, entry) in element.HierarchyScenesStructure) {
			var itemElement = new XElement("Item", new XAttribute("key", key));

			foreach (var (type, children) in entry) {
				if (children.Length == 0) continue;
				var container = new XElement(type.Serialize(),
					new XAttribute("count", children.Length),
					children.Select(id => new XElement("Item", id.ToString()))
				);
				itemElement.Add(container);
			}

			structureElement.Add(itemElement);
		}

		return structureElement;
	}
}
