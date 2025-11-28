using System.Collections.Generic;
using System.Xml.Linq;
using P2XMLEditor.GameData.Templates.InternalTypes.Interfaces;

namespace P2XMLEditor.GameData.Templates.InternalTypes.Components;

public struct AbilitiesComponent() : ITemplateComponent {
	public List<string> Abilities { get; } = [];

	public void LoadFromXml(XElement element) {
		var abilitiesEl = element.Element("Abilities");
		if (abilitiesEl != null) {
			foreach (var item in abilitiesEl.Elements("Item")) {
				var id = item.Element("Id")!.Value;
				Abilities.Add(id);
			}
		}
	}

	public XElement ToXml(XElement baseElement) {
		var abilitiesEl = new XElement("Abilities");
		foreach (var id in Abilities) {
			abilitiesEl.Add(new XElement("Item",
				new XElement("Id", id)
			));
		}

		baseElement.Add(abilitiesEl);
		return baseElement;
	}
}