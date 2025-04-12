using System.Xml.Linq;
using P2XMLEditor.GameData.Templates.InternalTypes.Abstract;
using static P2XMLEditor.Helper.XmlParsingHelper;

namespace P2XMLEditor.GameData.Templates.InternalTypes.Components;

public class NavigationComponent : TemplateComponent {
	public override string Type => "NavigationComponent";
	public bool IsEnabled { get; set; }

	public override void LoadFromXml(XElement element) {
		IsEnabled = ParseBool(element.Element("IsEnabled")!);
	}

	public override XElement ToXml() {
		var element = base.ToXml();
		element.Add(CreateBoolElement("IsEnabled", IsEnabled));
		return element;
	}
}