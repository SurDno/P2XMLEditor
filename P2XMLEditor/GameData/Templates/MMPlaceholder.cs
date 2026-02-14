using System.Xml.Linq;
using P2XMLEditor.GameData.Templates.Abstract;

namespace P2XMLEditor.GameData.Templates;

public class MMPlaceholder : TemplateObject {
	public string? ImageId { get; set; }
	
	public override void LoadFromXml(XElement element) {
		base.LoadFromXml(element);
		ImageId = element.Element("Image")!.Element("Id")!.Value;
	}

	public override XElement ToXml() {
		var element = base.ToXml();
		var img = new XElement("Image");
		img.Add(new XElement("Id", ImageId  ?? string.Empty));
		element.Add(img);
		return element;
	}
}