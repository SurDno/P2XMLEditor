using System.Xml.Linq;
using P2XMLEditor.GameData.Templates.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.Templates.InternalTypes.Components;

public class PositionComponent : TemplateComponent {
	public override string Type => "PositionComponent";
	public string Position { get; set; }
	public string Rotation { get; set; }

	public override void LoadFromXml(XElement element) {
		Position = element.Element("Position")?.Value ?? string.Empty;
		Rotation = element.Element("Rotation")?.Value ?? string.Empty;
	}

	public override XElement ToXml() {
		var element = base.ToXml();
		element.Add(new XElement("Position", Position));
		element.Add(new XElement("Rotation", Rotation));
		return element;
	}
}