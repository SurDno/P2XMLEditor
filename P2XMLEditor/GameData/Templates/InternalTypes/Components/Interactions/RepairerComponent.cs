using System.Xml.Linq;
using P2XMLEditor.GameData.Templates.Enums;
using P2XMLEditor.GameData.Templates.InternalTypes.Abstract;
using P2XMLEditor.Helper;

namespace P2XMLEditor.GameData.Templates.InternalTypes.Components;

public struct RepairerComponent() : ITemplateComponent {
    public List<StorableGroup> RepairableGroups { get; } = [];

    public void LoadFromXml(XElement element) {
        var groupsEl = element.Element("RepairableGroups");
        foreach (var item in groupsEl!.Elements("Item")) 
            RepairableGroups.Add(item.Value.Deserialize<StorableGroup>());
    }


    public XElement ToXml(XElement baseElement) {
        var g = new XElement("RepairableGroups");
        foreach (var grp in RepairableGroups) 
            g.Add(new XElement("Item", grp.Serialize()));
        baseElement.Add(g);
        return baseElement;
    }
}