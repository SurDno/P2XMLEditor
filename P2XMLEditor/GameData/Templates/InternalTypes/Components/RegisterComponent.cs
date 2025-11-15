using System.Xml.Linq;
using P2XMLEditor.GameData.Templates.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.Templates.InternalTypes.Components;

public struct RegisterComponent() : ITemplateComponent {
	public string Tag { get; set; }

	public void LoadFromXml(XElement element) {
		Tag = element.Element("Tag")!.Value;
	}

	public XElement ToXml(XElement baseElement) {
		baseElement.Add(new XElement("Tag", Tag));
		return baseElement;
	}
}
