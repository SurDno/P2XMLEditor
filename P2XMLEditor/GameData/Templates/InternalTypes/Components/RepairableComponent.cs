using System.Xml.Linq;
using P2XMLEditor.GameData.Templates.InternalTypes.Interfaces;

namespace P2XMLEditor.GameData.Templates.InternalTypes.Components;

public struct RepairableComponent : ITemplateComponent {
	public string SettingsId { get; set; }

	public void LoadFromXml(XElement element) {
		SettingsId = element.Element("Settings")!.Element("Id")!.Value;
	}

	public XElement ToXml(XElement baseElement) { 
		baseElement.Add(new XElement("Settings", new XElement("Id", SettingsId))); 
		return baseElement;
	}
}