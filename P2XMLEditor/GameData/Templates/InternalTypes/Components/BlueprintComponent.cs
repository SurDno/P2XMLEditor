using System.Xml.Linq;
using P2XMLEditor.GameData.Templates.InternalTypes.Interfaces;
using static P2XMLEditor.Helper.XmlParsingHelper;

namespace P2XMLEditor.GameData.Templates.InternalTypes.Components;

public struct BlueprintComponent : ITemplateComponent, IEnableable {
	public bool IsEnabled { get; set; }
	public string? BlueprintId { get; set; }

	public void LoadFromXml(XElement element) {
		IsEnabled = ParseBool(element.Element("IsEnabled")!);

		var bp = element.Element("Blueprint");
		if (bp != null) {
			var id = bp.Element("Id")?.Value;
			BlueprintId = string.IsNullOrEmpty(id) ? null : id;
		}
	}

	public XElement ToXml(XElement baseElement) {
		baseElement.Add(CreateBoolElement("IsEnabled", IsEnabled));

		var bp = new XElement("Blueprint");
		bp.Add(new XElement("Id", BlueprintId ?? string.Empty));
		baseElement.Add(bp);

		return baseElement;
	}
}
