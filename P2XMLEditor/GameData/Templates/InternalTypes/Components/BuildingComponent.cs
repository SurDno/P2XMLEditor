using System.Xml.Linq;
using P2XMLEditor.GameData.Templates.Enums;
using P2XMLEditor.GameData.Templates.InternalTypes.Abstract;
using P2XMLEditor.Helper;

namespace P2XMLEditor.GameData.Templates.InternalTypes.Components;

public struct BuildingComponent() : ITemplateComponent {
	public BuildingType Building { get; set; } = BuildingType.None;

	public void LoadFromXml(XElement element) {
		Building = element.Element("Building")!.Value.Deserialize<BuildingType>();
	}

	 public XElement ToXml(XElement baseElement) { 
		 baseElement.Add(new XElement("Building", Building.Serialize())); 
		 return baseElement;
	}
}