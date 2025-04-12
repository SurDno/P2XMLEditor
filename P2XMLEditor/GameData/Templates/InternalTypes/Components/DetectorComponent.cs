using System.Xml.Linq;
using P2XMLEditor.GameData.Templates.InternalTypes.Abstract;
using static P2XMLEditor.Helper.XmlParsingHelper;

namespace P2XMLEditor.GameData.Templates.InternalTypes.Components;

public class DetectorComponent : TemplateComponent {
    public override string Type => "DetectorComponent";
    public bool IsEnabled { get; set; } = true;

    public override void LoadFromXml(XElement element) {
        IsEnabled = ParseBool(element.Element("IsEnabled")!);
    }

    public override XElement ToXml() {
        var element = base.ToXml();
        element.Add(CreateBoolElement("IsEnabled", IsEnabled));
        return element;
    }
}