using System.Xml.Linq;
using P2XMLEditor.GameData.Templates.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.Templates.InternalTypes.Components;

public struct StorageComponent() : ITemplateComponent {
	public struct InventoryEntry {
		public string Id;
		public string InventoryTemplateId;
	}

	public string Tag { get; set; }
	public List<InventoryEntry> InventoryTemplates { get; } = [];

	public void LoadFromXml(XElement element) {
		Tag = element.Element("Tag")!.Value;

		var invEl = element.Element("InventoryTemplates");
		if (invEl != null) {
			foreach (var item in invEl.Elements("Item")) {
				var entry = new InventoryEntry {
					Id = item.Element("Id")!.Value,
					InventoryTemplateId = item.Element("InventoryTemplate")!.Element("Id")!.Value
				};
				InventoryTemplates.Add(entry);
			}
		}
	}

	public XElement ToXml(XElement baseElement) {
		baseElement.Add(new XElement("Tag", Tag));

		var inv = new XElement("InventoryTemplates");
		foreach (var entry in InventoryTemplates) {
			var it = new XElement("Item");
			it.Add(new XElement("Id", entry.Id));
			it.Add(new XElement("InventoryTemplate", new XElement("Id", entry.InventoryTemplateId)));
			inv.Add(it);
		}

		baseElement.Add(inv);
		return baseElement;
	}
}