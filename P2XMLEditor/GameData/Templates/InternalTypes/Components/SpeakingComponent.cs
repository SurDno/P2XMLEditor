using System.Xml.Linq;
using P2XMLEditor.GameData.Templates.InternalTypes.Interfaces;
using static P2XMLEditor.Helper.XmlParsingHelper;

namespace P2XMLEditor.GameData.Templates.InternalTypes.Components;

public struct SpeakingComponent() : ITemplateComponent {
	public bool IsEnabled { get; set; }

	public void LoadFromXml(XElement element) {
		IsEnabled = ParseBool(element.Element("IsEnabled")!);
	}

	 public XElement ToXml(XElement baseElement) {
		 baseElement.Add(CreateBoolElement("IsEnabled", IsEnabled)); 
		 return baseElement;
	}
}