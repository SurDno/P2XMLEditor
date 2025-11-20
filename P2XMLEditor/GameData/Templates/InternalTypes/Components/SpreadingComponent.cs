using System.Xml.Linq;
using P2XMLEditor.GameData.Enums;
using P2XMLEditor.GameData.Templates.InternalTypes.Abstract;
using P2XMLEditor.Helper;

namespace P2XMLEditor.GameData.Templates.InternalTypes.Components;

public struct SpreadingComponent() : ITemplateComponent {
	public DiseasedStateType DiseasedState { get; set; } = DiseasedStateType.Normal;

	public void LoadFromXml(XElement element) {
		DiseasedState = element.Element("DiseasedState")!.Value.Deserialize<DiseasedStateType>();
	}

	 public XElement ToXml(XElement baseElement) {
		 baseElement.Add(new XElement("DiseasedState", DiseasedState.Serialize())); 
		 return baseElement;
	}
}