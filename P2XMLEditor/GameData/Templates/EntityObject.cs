using P2XMLEditor.GameData.Templates.Abstract;
using System.Xml.Linq;
using P2XMLEditor.GameData.Templates.InternalTypes.Abstract;
using P2XMLEditor.Helper;
using P2XMLEditor.Logging;
using static P2XMLEditor.Helper.XmlParsingHelper;

namespace P2XMLEditor.GameData.Templates;

public class EntityObject : TemplateObject {
	public override string Type => "Entity";
	public List<TemplateComponent> Components { get; set; } = [];
	public bool IsEnabled { get; set; } = true;

	public override void LoadFromXml(XElement element) {
		base.LoadFromXml(element);

		var componentsElement = element.Element("Components");
		if (componentsElement != null) {
			foreach (var componentElement in componentsElement.Elements("Item")) {
				var type = componentElement.Attribute("type")?.Value;
				try {
					var component = TemplateComponent.CreateComponent(type);
					if (component == null) continue;
					component.LoadFromXml(componentElement);
					Components.Add(component);
				} catch (Exception e) {
					Logger.Log(LogLevel.Error, $"{e.Message}");
				}
			}
		}

		IsEnabled = ParseBool(element.Element("IsEnabled")!);
	}

	public override XElement ToXml() {
		var element = base.ToXml();

		var componentsElement = new XElement("Components");
		foreach (var component in Components) 
			componentsElement.Add(component.ToXml());
		
		element.Add(componentsElement);
		element.Add(CreateBoolElement("IsEnabled", IsEnabled));

		return element;
	}
}