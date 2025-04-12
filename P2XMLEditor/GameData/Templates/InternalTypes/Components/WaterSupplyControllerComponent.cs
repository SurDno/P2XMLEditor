using System.Xml.Linq;
using P2XMLEditor.GameData.Templates.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.Templates.InternalTypes.Components;

public class WaterSupplyControllerComponent : TemplateComponent {
	public override string Type => "WaterSupplyControllerComponent";
	
	public override void LoadFromXml(XElement element) { }
}