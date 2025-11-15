using System.Xml.Linq;
using P2XMLEditor.GameData.Templates.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.Templates.InternalTypes.Components;

public struct BehaviorComponent() : ITemplateComponent {
	public string? BehaviorTreeId { get; set; }

	public void LoadFromXml(XElement element) {
		var bt = element.Element("BehaviorTree");
		if (bt != null) {
			var idEl = bt.Element("Id");
			BehaviorTreeId = string.IsNullOrEmpty(idEl?.Value) ? null : idEl!.Value;
		}
	}

	public XElement ToXml(XElement baseElement) {
		var bt = new XElement("BehaviorTree");
		bt.Add(new XElement("Id", BehaviorTreeId ?? string.Empty));
		baseElement.Add(bt);
		return baseElement;
	}
}