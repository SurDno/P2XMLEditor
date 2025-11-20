using System.Xml.Linq;

namespace P2XMLEditor.GameData.Templates.Abstract;

public abstract class TemplateObject {
	public string Name { get; set; }
	public Guid Id { get; set; }

	public virtual void LoadFromXml(XElement element) {
		Name = element.Element("Name")?.Value ?? string.Empty;
		Id = new Guid(element.Element("Id")?.Value ?? throw new Exception("Id cannot be null."));
	}

	public virtual XElement ToXml() {
		var element = new XElement("Object", new XAttribute("type", nameof(Type)));
		element.Add(new XElement("Name", Name));
		element.Add(new XElement("Id", Id));
		return element;
	}
}