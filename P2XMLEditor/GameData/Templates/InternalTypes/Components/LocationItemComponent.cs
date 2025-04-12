using System.Xml.Linq;
using P2XMLEditor.GameData.Templates.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.Templates.InternalTypes.Components;

public class LocationItemComponent : TemplateComponent {
	public override string Type => "LocationItemComponent";
	
	public override void LoadFromXml(XElement element) { }
}