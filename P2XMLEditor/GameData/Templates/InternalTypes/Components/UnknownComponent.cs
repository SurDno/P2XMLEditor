using System.Xml.Linq;
using P2XMLEditor.GameData.Templates.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.Templates.InternalTypes.Components;

public class UnknownComponent : TemplateComponent {
	private readonly string _typeName;
	private readonly Dictionary<string, string> _properties = new();


	private XElement fullEl;
	
	public UnknownComponent(string typeName) {
		_typeName = typeName;
	}

	public override string Type => _typeName;

	public override void LoadFromXml(XElement element) {
		fullEl = element;
	}

	public override XElement ToXml() {
		return fullEl;
	}
}