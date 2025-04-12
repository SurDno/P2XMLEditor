using System.Xml.Linq;
using P2XMLEditor.GameData.Templates.InternalTypes.Components;

namespace P2XMLEditor.GameData.Templates.InternalTypes.Abstract;

public abstract class TemplateComponent {
    public abstract string Type { get; }

    public abstract void LoadFromXml(XElement element);

    public virtual XElement ToXml() {
        return new XElement("Item", new XAttribute("type", Type));
    }

    public static TemplateComponent? CreateComponent(string? type) => type switch {
	    "StaticModelComponent" => new StaticModelComponent(),
	    "LocationComponent" => new LocationComponent(),
	    "CrowdPointsComponent" => new CrowdPointsComponent(),
	    "TagsComponent" => new TagsComponent(),
	    "DiseaseComponent" => new DiseaseComponent(),
	    "WaterSupplyControllerComponent" => new WaterSupplyControllerComponent(),
	    "LipSyncComponent" => new LipSyncComponent(),
	    "SpeakingComponent" => new SpeakingComponent(),
	    "NavigationComponent" => new NavigationComponent(),
	    "PositionComponent" => new PositionComponent(),
	    "BuildingComponent" => new BuildingComponent(),
	    "LocationItemComponent" => new LocationItemComponent(),
	    "SpreadingComponent" => new SpreadingComponent(),
	    "TriggerComponent" => new TriggerComponent(),
	    "DetectorComponent" => new DetectorComponent(),
	    _ => throw new ArgumentException($"Invalid component type: {type}")
    };
}