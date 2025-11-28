using System.Xml.Linq;
using P2XMLEditor.GameData.Templates.InternalTypes.Interfaces;
using static P2XMLEditor.Helper.XmlParsingHelper;

namespace P2XMLEditor.GameData.Templates.InternalTypes.Components;


// TODO: store PlaceholderId as GUID or direct class ref
public struct MapItemComponent : ITemplateComponent, IEnableable {
	public bool IsEnabled { get; set; }
	public string PlaceholderId { get; set; }

	public void LoadFromXml(XElement element) {
		IsEnabled = ParseBool(element.Element("IsEnabled")!);
		PlaceholderId = element.Element("Placeholder")!.Element("Id")!.Value;
	}

	 public XElement ToXml(XElement baseElement) {
		baseElement.Add(CreateBoolElement("IsEnabled", IsEnabled));
		baseElement.Add(new XElement("Connection", new XElement("Id", PlaceholderId)));
		return baseElement;
	}
}