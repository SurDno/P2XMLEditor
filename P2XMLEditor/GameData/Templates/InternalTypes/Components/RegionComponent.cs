using System.Xml.Linq;
using P2XMLEditor.Enums;
using P2XMLEditor.Enums.Templates;
using P2XMLEditor.GameData.Templates.InternalTypes.Interfaces;

namespace P2XMLEditor.GameData.Templates.InternalTypes.Components;

public struct RegionComponent : ITemplateComponent {
	public RegionEnum Region { get; set; }
	public RegionBehaviourEnum RegionBehavior { get; set; }

	public void LoadFromXml(XElement element) {
		Region = element.Element("Region")!.Value.Deserialize<RegionEnum>();
		RegionBehavior = element.Element("RegionBehavior")!.Value.Deserialize<RegionBehaviourEnum>();
	}

	public XElement ToXml(XElement baseElement) {
		baseElement.Add(new XElement("Region", Region.Serialize()));
		baseElement.Add(new XElement("RegionBehavior", RegionBehavior.Serialize()));
		return baseElement;
	}
}