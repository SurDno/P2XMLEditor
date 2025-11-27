using System.Xml.Linq;
using P2XMLEditor.GameData.Templates.InternalTypes.Interfaces;

namespace P2XMLEditor.GameData.Templates.InternalTypes.Components;

public struct InventoryComponent() : ITemplateComponent {
	public string? ContainerResourceId { get; set; }

	public void LoadFromXml(XElement element) {
		var cr = element.Element("ContainerResource");
		if (cr != null) {
			var id = cr.Element("Id")?.Value;
			ContainerResourceId = string.IsNullOrEmpty(id) ? null : id;
		}
	}

	public XElement ToXml(XElement baseElement) {
		var cr = new XElement("ContainerResource");
		cr.Add(new XElement("Id", ContainerResourceId ?? string.Empty));
		baseElement.Add(cr);
		return baseElement;
	}
}
