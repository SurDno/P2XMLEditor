using System.Xml.Linq;
using P2XMLEditor.Enums;
using P2XMLEditor.Enums.Templates;
using P2XMLEditor.GameData.Templates.InternalTypes.Interfaces;

namespace P2XMLEditor.GameData.Templates.InternalTypes.Components;

public struct SpawnpointComponent : ITemplateComponent {
	public Kind Type { get; set; }

	public void LoadFromXml(XElement element) {
		Type = element.Element("Type")!.Value.Deserialize<Kind>();
	}

	public XElement ToXml(XElement baseElement) {
		baseElement.Add(new XElement("Type", Type.Serialize()));
		return baseElement;
	}
}