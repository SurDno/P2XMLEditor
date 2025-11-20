using System.Xml.Linq;

namespace P2XMLEditor.GameData.Templates.InternalTypes.Abstract;

public interface ITemplateComponent {
    public void LoadFromXml(XElement element);
    public XElement ToXml(XElement baseElement);
}