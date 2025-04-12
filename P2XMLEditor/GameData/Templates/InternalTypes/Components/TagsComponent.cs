using System.Xml.Linq;
using P2XMLEditor.GameData.Templates.Enums;
using P2XMLEditor.GameData.Templates.InternalTypes.Abstract;
using P2XMLEditor.Helper;

namespace P2XMLEditor.GameData.Templates.InternalTypes.Components;

public class TagsComponent : TemplateComponent {
	public override string Type => "TagsComponent";
	public List<EntityTag> Tags { get; set; } = [];

	public override void LoadFromXml(XElement element) {
		foreach (var item in element.Element("Tags")!.Elements("Item")) 
			Tags.Add(item.Value.Deserialize<EntityTag>());
	}

	public override XElement ToXml() {
		var element = base.ToXml();
		var tagsElement = new XElement("Tags");
		foreach (var tag in Tags)
			tagsElement.Add(new XElement("Item", tag.Serialize()));

		element.Add(tagsElement);
		return element;
	}
}