using System.Xml.Linq;
using P2XMLEditor.GameData.Templates.InternalTypes.Abstract;

namespace P2XMLEditor.GameData.Templates.InternalTypes.Components;

public struct EffectsComponent() : ITemplateComponent {
	
	public void LoadFromXml(XElement element) { }
	public XElement ToXml(XElement baseElement) => baseElement;
}