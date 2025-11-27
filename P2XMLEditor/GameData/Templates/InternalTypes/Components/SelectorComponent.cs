using System.Xml.Linq;
using P2XMLEditor.GameData.Templates.InternalTypes.Interfaces;

namespace P2XMLEditor.GameData.Templates.InternalTypes.Components;

public struct SelectorComponent() : ITemplateComponent {
	public List<SelectorPreset> Presets { get; } = [];

	public struct SelectorPreset {
		public List<string> Objects;
	}

	public void LoadFromXml(XElement element) {
		foreach (var presetItem in element.Element("Presets")!.Elements("Item")) {
			var objects = presetItem.Element("Objects")!.Elements("Item").Select(i => i.Element("Id")!.Value);
			Presets.Add(new SelectorPreset { Objects = objects.ToList() }); 
		}
	}

	public XElement ToXml(XElement baseElement) {
		var presetsEl = new XElement("Presets");
		foreach (var preset in Presets) {
			var presetItem = new XElement("Item");
			var objectsEl = new XElement("Objects");
			foreach (var id in preset.Objects) 
				objectsEl.Add(new XElement("Item", new XElement("Id", id)));
			presetItem.Add(objectsEl);
			presetsEl.Add(presetItem);
		}
		baseElement.Add(presetsEl);
		return baseElement;
	}
}
