using System.Xml.Linq;
using P2XMLEditor.GameData.Templates.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.Templates.InternalTypes.Components;

public class DiseaseComponent : TemplateComponent {
	public override string Type => "DiseaseComponent";

	public override void LoadFromXml(XElement element) { }
}