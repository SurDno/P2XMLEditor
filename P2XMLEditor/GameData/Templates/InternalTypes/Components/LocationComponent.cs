using System.Xml.Linq;
using P2XMLEditor.GameData.Templates.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.Templates.InternalTypes.Components;

public class LocationComponent : TemplateComponent {
	public override string Type => "LocationComponent";
	public string LocationType { get; set; } = "None";

	public override void LoadFromXml(XElement element) {
		LocationType = element.Element("LocationType")?.Value ?? "None";
	}

	public override XElement ToXml() {
		var element = base.ToXml();
		element.Add(new XElement("LocationType", LocationType));
		return element;
	}
}