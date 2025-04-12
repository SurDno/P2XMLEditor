using System.Xml.Linq;
using P2XMLEditor.GameData.Templates.InternalTypes.Abstract;
using static P2XMLEditor.Helper.XmlParsingHelper;

namespace P2XMLEditor.GameData.Templates.InternalTypes.Components;

public class StaticModelComponent : TemplateComponent {
	public override string Type => "StaticModelComponent";
	public bool RelativePosition { get; set; }
	public string ConnectionId { get; set; }

	public override void LoadFromXml(XElement element) {
		RelativePosition = ParseBool(element.Element("RelativePosition")!);
		ConnectionId = element.Element("Connection")?.Element("Id")?.Value ?? string.Empty;
	}

	public override XElement ToXml() {
		var element = base.ToXml();
		element.Add(CreateBoolElement("RelativePosition", RelativePosition));
		if (!string.IsNullOrEmpty(ConnectionId)) {
			element.Add(new XElement("Connection",
				new XElement("Id", ConnectionId)));
		}

		return element;
	}
}