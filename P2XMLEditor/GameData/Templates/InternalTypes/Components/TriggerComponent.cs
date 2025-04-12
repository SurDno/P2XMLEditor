using System.Xml.Linq;
using P2XMLEditor.GameData.Templates.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.Templates.InternalTypes.Components;

public class TriggerComponent : TemplateComponent {
	public override string Type => "TriggerComponent";
	
	public override void LoadFromXml(XElement element) { }
}
