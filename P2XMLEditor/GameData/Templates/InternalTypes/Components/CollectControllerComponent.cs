using System.Xml.Linq;
using P2XMLEditor.GameData.Templates.InternalTypes.Abstract;
using static P2XMLEditor.Helper.XmlParsingHelper;

namespace P2XMLEditor.GameData.Templates.InternalTypes.Components;

public struct CollectControllerComponent() : ITemplateComponent {
	public string? StorableId { get; set; }
	public bool SendActionEvent { get; set; }

	public void LoadFromXml(XElement element) {
		var st = element.Element("Storable");
		if (st != null) {
			var id = st.Element("Id")?.Value;
			StorableId = string.IsNullOrEmpty(id) ? null : id;
		}

		SendActionEvent = ParseBool(element.Element("SendActionEvent")!);
	}

	public XElement ToXml(XElement baseElement) {
		var st = new XElement("Storable");
		st.Add(new XElement("Id", StorableId ?? string.Empty));
		baseElement.Add(st);

		baseElement.Add(new XElement("SendActionEvent", SendActionEvent));
		return baseElement;
	}
}