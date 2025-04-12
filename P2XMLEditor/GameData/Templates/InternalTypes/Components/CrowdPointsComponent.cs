using System.Xml.Linq;
using P2XMLEditor.GameData.Templates.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.Templates.InternalTypes.Components;

public class CrowdPointsComponent : TemplateComponent {
	public override string Type => "CrowdPointsComponent";
	
	public override void LoadFromXml(XElement element) { }
}