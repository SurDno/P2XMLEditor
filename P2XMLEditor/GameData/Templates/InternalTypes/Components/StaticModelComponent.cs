using System.Xml.Linq;
using P2XMLEditor.GameData.Templates.InternalTypes.Interfaces;
using static P2XMLEditor.Helper.XmlParsingHelper;

namespace P2XMLEditor.GameData.Templates.InternalTypes.Components;

// TODO: store ConnectionId as GUID or direct class ref
public struct StaticModelComponent() : ITemplateComponent {
	public bool RelativePosition { get; set; }
	public string ConnectionId { get; set; }

	public void LoadFromXml(XElement element) {
		RelativePosition = ParseBool(element.Element("RelativePosition")!);
		ConnectionId = element.Element("Connection")?.Element("Id")?.Value ?? string.Empty;
	}

	 public XElement ToXml(XElement baseElement) { 
		 baseElement.Add(CreateBoolElement("RelativePosition", RelativePosition)); 
		 if (!string.IsNullOrEmpty(ConnectionId)) 
			 baseElement.Add(new XElement("Connection", new XElement("Id", ConnectionId))); 
		 return baseElement;
	}
}