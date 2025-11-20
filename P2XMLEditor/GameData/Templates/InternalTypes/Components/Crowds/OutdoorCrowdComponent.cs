using System.Xml.Linq;
using P2XMLEditor.GameData.Templates.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.Templates.InternalTypes.Components;

public struct OutdoorCrowdComponent() : ITemplateComponent {
	public string? DataId { get; set; }

	public void LoadFromXml(XElement element) {
		var data = element.Element("Data");
		if (data != null) {
			var id = data.Element("Id")?.Value;
			DataId = string.IsNullOrEmpty(id) ? null : id;
		}
	}

	public XElement ToXml(XElement baseElement) {
		var data = new XElement("Data");
		data.Add(new XElement("Id", DataId ?? string.Empty));
		baseElement.Add(data);
		return baseElement;
	}
}