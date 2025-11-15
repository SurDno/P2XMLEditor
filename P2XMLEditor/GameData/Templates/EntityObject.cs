using System.Collections.Concurrent;
using P2XMLEditor.GameData.Templates.Abstract;
using System.Xml.Linq;
using P2XMLEditor.GameData.Templates.InternalTypes.Abstract;
using P2XMLEditor.GameData.Templates.InternalTypes.Components;
using P2XMLEditor.Helper;
using P2XMLEditor.Logging;
using static P2XMLEditor.Helper.XmlParsingHelper;

namespace P2XMLEditor.GameData.Templates;

public class EntityObject : TemplateObject {
	public override string Type => "Entity";
	public List<ITemplateComponent> Components { get; set; } = [];
	public bool IsEnabled { get; set; } = true;
	
	public static ConcurrentDictionary<string,int> invalidComponent = new();

	public override void LoadFromXml(XElement element) {
		base.LoadFromXml(element);

		var componentsElement = element.Element("Components");

		if (componentsElement != null) {
			foreach (var componentElement in componentsElement.Elements("Item")) {
				var type = componentElement.Attribute("type")!.Value;
				var component = CreateComponent(type);
				if (component != null) {
					component.LoadFromXml(componentElement);
					Components.Add(component);
				} else {
					if (!invalidComponent.TryAdd(type, 1))
						invalidComponent[type]++;
				}
			}
		}

		IsEnabled = ParseBool(element.Element("IsEnabled")!);
	}
	
	public static ITemplateComponent? CreateComponent(string? type) => type switch {
		nameof(StaticModelComponent) => new StaticModelComponent(),
		nameof(LocationComponent) => new LocationComponent(),
		nameof(CrowdPointsComponent) => new CrowdPointsComponent(),
		nameof(TagsComponent) => new TagsComponent(),
		nameof(DiseaseComponent) => new DiseaseComponent(),
		nameof(WaterSupplyControllerComponent) => new WaterSupplyControllerComponent(),
		nameof(LipSyncComponent) => new LipSyncComponent(),
		nameof(SpeakingComponent) => new SpeakingComponent(),
		nameof(NavigationComponent) => new NavigationComponent(),
		nameof(PositionComponent) => new PositionComponent(),
		nameof(BuildingComponent) => new BuildingComponent(),
		nameof(LocationItemComponent) => new LocationItemComponent(),
		nameof(SpreadingComponent) => new SpreadingComponent(),
		nameof(TriggerComponent) => new TriggerComponent(),
		nameof(DetectorComponent) => new DetectorComponent(),
		nameof(MapItemComponent) => new MapItemComponent(),
		nameof(StorableComponent) => new StorableComponent(),
		nameof(MailComponent) => new MailComponent(),
		nameof(FastTravelComponent) => new FastTravelComponent(),
		nameof(MarketComponent) => new MarketComponent(),
		nameof(DetectableComponent) => new DetectableComponent(),
		nameof(TestComponent) => new TestComponent(),
		nameof(RepairerComponent) => new RepairerComponent(),
		nameof(RepairableComponent) => new RepairableComponent(),
		nameof(ParentComponent) => new ParentComponent(),	
		nameof(ContextComponent) => new ContextComponent(),
		nameof(HerbRootsComponent) => new HerbRootsComponent(),
		nameof(EffectsComponent) => new EffectsComponent(),
		nameof(InteractableComponent) => new InteractableComponent(),
		nameof(BehaviorComponent) => new BehaviorComponent(),
		nameof(BlueprintComponent) => new BlueprintComponent(),
		nameof(RegisterComponent) => new RegisterComponent(),
		nameof(AbilitiesComponent) => new AbilitiesComponent(),
		nameof(RegionComponent) => new RegionComponent(),
		_ => null
	};

	public override XElement ToXml() {
		var element = base.ToXml();

		var componentsElement = new XElement("Components");
		foreach (var component in Components) 
			componentsElement.Add(component.ToXml(new XElement("Item", new XAttribute("type", Type))));

		element.Add(componentsElement);
		element.Add(CreateBoolElement("IsEnabled", IsEnabled));

		return element;
	}
}