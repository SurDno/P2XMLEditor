using System.Xml.Linq;
using P2XMLEditor.GameData.Templates.InternalTypes.Interfaces;

namespace P2XMLEditor.GameData.Templates.InternalTypes.Components;

public struct MessangerComponent() : ITemplateComponent {
	
	public void LoadFromXml(XElement element) { }
	public XElement ToXml(XElement baseElement) => baseElement;
}
