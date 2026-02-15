using System.Xml.Linq;
using P2XMLEditor.GameData.Enums;
using P2XMLEditor.GameData.Templates.InternalTypes.Interfaces;
using P2XMLEditor.Helper;

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