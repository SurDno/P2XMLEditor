using System.Xml.Linq;
using P2XMLEditor.GameData.Enums;
using P2XMLEditor.GameData.Templates.InternalTypes.Abstract;
using P2XMLEditor.Helper;

namespace P2XMLEditor.GameData.Templates.InternalTypes.Components;

public struct TagsComponent() : ITemplateComponent {
	public List<EntityTag> Tags { get; set; } = [];

	public void LoadFromXml(XElement element) {
		foreach (var item in element.Element("Tags")!.Elements("Item")) 
			Tags.Add(item.Value.Deserialize<EntityTag>());
	}

	 public XElement ToXml(XElement baseElement) {
		var tagsElement = new XElement("Tags");
		foreach (var tag in Tags)
			tagsElement.Add(new XElement("Item", tag.Serialize()));

		baseElement.Add(tagsElement);
		return baseElement;
	}
}