using System.Xml.Linq;
using P2XMLEditor.GameData.Templates.InternalTypes.Interfaces;

namespace P2XMLEditor.GameData.Templates.InternalTypes.Components;

// TODO: implement for storing custom modded components.
public struct UnknownComponent(string typeName) : ITemplateComponent {
	private XElement fullEl;

	public void LoadFromXml(XElement element) {
		fullEl = element;
	}

	 public XElement ToXml(XElement baseElement) {
		return fullEl;
	}
}