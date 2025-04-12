using System.Xml.Linq;
using P2XMLEditor.GameData.Templates.Enums;
using P2XMLEditor.GameData.Templates.InternalTypes.Abstract;
using P2XMLEditor.Helper;

namespace P2XMLEditor.GameData.Templates.InternalTypes.Components;

public class SpreadingComponent : TemplateComponent {
	public override string Type => "SpreadingComponent";
	public DiseasedStateType DiseasedState { get; set; } = DiseasedStateType.Normal;

	public override void LoadFromXml(XElement element) {
		DiseasedState = element.Element("DiseasedState")!.Value.Deserialize<DiseasedStateType>();
	}

	public override XElement ToXml() {
		var element = base.ToXml();
		element.Add(new XElement("DiseasedState", DiseasedState.Serialize()));
		return element;
	}
}