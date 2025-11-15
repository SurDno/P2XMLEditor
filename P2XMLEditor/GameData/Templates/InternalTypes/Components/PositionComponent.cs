using System.Xml.Linq;
using P2XMLEditor.GameData.Templates.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.Templates.InternalTypes.Components;

// TODO: store actual vector3s instead of strings?
public struct PositionComponent() : ITemplateComponent {
	public string Position { get; set; }
	public string Rotation { get; set; }

	public void LoadFromXml(XElement element) {
		Position = element.Element("Position")?.Value ?? string.Empty;
		Rotation = element.Element("Rotation")?.Value ?? string.Empty;
	}

	 public XElement ToXml(XElement baseElement) {
		 baseElement.Add(new XElement("Position", Position));
		 baseElement.Add(new XElement("Rotation", Rotation)); 
		 return baseElement;
	}
}