using System.Xml.Linq;
using P2XMLEditor.GameData.Templates.InternalTypes.Abstract;
using static P2XMLEditor.Helper.XmlParsingHelper;

namespace P2XMLEditor.GameData.Templates.InternalTypes.Components;

public struct DynamicModelComponent() : ITemplateComponent {
	public bool IsEnabled { get; set; }
	public string? ModelId { get; set; }
	public List<string> Models { get; } = [];

	public void LoadFromXml(XElement element) {
		IsEnabled = ParseBool(element.Element("IsEnabled")!);

		var modelEl = element.Element("Model");
		if (modelEl != null) {
			var id = modelEl.Element("Id")?.Value;
			ModelId = string.IsNullOrEmpty(id) ? null : id;
		}

		var modelsEl = element.Element("Models");
		if (modelsEl != null) {
			foreach (var item in modelsEl.Elements("Item")) {
				var id = item.Element("Id")!.Value;
				Models.Add(id);
			}
		}
	}

	public XElement ToXml(XElement baseElement) {
		baseElement.Add(CreateBoolElement("IsEnabled", IsEnabled));

		var modelEl = new XElement("Model");
		modelEl.Add(new XElement("Id", ModelId ?? string.Empty));
		baseElement.Add(modelEl);

		var modelsEl = new XElement("Models");
		foreach (var id in Models) {
			modelsEl.Add(new XElement("Item",
				new XElement("Id", id)
			));
		}
		baseElement.Add(modelsEl);

		return baseElement;
	}
}
