using System.Xml.Linq;
using P2XMLEditor.GameData.Templates.InternalTypes.Abstract;
using P2XMLEditor.GameData.Types;
using static P2XMLEditor.Helper.XmlParsingHelper;

namespace P2XMLEditor.GameData.Templates.InternalTypes.Components;

// TODO: store actual vector3s instead of strings?
public struct PositionComponent() : ITemplateComponent {
	public Vector3 Position { get; set; }
	public Vector3 Rotation { get; set; }

	public void LoadFromXml(XElement element) {
		Position = element.Element("Position")!.ParseVector3();
		Rotation = element.Element("Rotation")!.ParseVector3();
	}

	 public XElement ToXml(XElement baseElement) {
		 baseElement.Add(CreateVector3Element("Position", Position));
		 baseElement.Add(CreateVector3Element("Rotation", Rotation)); 
		 return baseElement;
	}
}