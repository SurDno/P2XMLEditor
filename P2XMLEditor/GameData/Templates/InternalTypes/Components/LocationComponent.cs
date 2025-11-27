using System.Xml.Linq;
using P2XMLEditor.GameData.Templates.InternalTypes.Interfaces;

namespace P2XMLEditor.GameData.Templates.InternalTypes.Components;

// TODO: store actual vector3s instead of strings?
public struct LocationComponent() : ITemplateComponent {
	public string LocationType { get; set; } = "None";

	public void LoadFromXml(XElement element) {
		LocationType = element.Element("LocationType")?.Value ?? "None";
	}

	 public XElement ToXml(XElement baseElement) {
		baseElement.Add(new XElement("LocationType", LocationType));
		return baseElement;
	}
}