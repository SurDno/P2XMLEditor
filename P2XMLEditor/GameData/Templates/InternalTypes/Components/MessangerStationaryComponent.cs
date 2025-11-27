using System.Xml.Linq;
using P2XMLEditor.GameData.Enums;
using P2XMLEditor.GameData.Templates.InternalTypes.Interfaces;
using P2XMLEditor.Helper;

namespace P2XMLEditor.GameData.Templates.InternalTypes.Components;

public struct MessangerStationaryComponent() : ITemplateComponent {
	public SpawnpointKind SpawnpointKindEnum { get; set; }

	public void LoadFromXml(XElement element) {
		SpawnpointKindEnum = element.Element("SpawnpointKindEnum")!.Value.Deserialize<SpawnpointKind>();
	}

	public XElement ToXml(XElement baseElement) {
		baseElement.Add(new XElement("SpawnpointKindEnum", SpawnpointKindEnum.Serialize()));
		return baseElement;
	}
}