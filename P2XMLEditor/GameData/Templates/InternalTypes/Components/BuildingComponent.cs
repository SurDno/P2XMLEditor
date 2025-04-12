using System.Xml.Linq;
using P2XMLEditor.GameData.Templates.Enums;
using P2XMLEditor.GameData.Templates.InternalTypes.Abstract;
using P2XMLEditor.Helper;

namespace P2XMLEditor.GameData.Templates.InternalTypes.Components;

public class BuildingComponent : TemplateComponent {
	public override string Type => "BuildingComponent";
	public BuildingType Building { get; set; } = BuildingType.None;

	public override void LoadFromXml(XElement element) {
		Building = element.Element("Building")!.Value.Deserialize<BuildingType>();
	}

	public override XElement ToXml() {
		var element = base.ToXml();
		element.Add(new XElement("Building", Building.Serialize()));
		return element;
	}
}