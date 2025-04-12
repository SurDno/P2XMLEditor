using System.Xml.Linq;
using P2XMLEditor.GameData.Templates.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.Templates.InternalTypes.Components;

public class LipSyncComponent : TemplateComponent {
	public override string Type => "LipSyncComponent";
	
	public override void LoadFromXml(XElement element) { }
}
