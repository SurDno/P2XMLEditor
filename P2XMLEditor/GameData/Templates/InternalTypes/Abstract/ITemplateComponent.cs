using System.Xml.Linq;
using P2XMLEditor.GameData.Templates.InternalTypes.Components;

namespace P2XMLEditor.GameData.Templates.InternalTypes.Abstract;

public interface ITemplateComponent {
    public void LoadFromXml(XElement element);
    public XElement ToXml(XElement baseElement);
}