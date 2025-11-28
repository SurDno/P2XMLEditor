using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Xml.Linq;
using P2XMLEditor.GameData.Templates.Abstract;
using P2XMLEditor.GameData.Templates.InternalTypes.Components;
using P2XMLEditor.GameData.Templates.InternalTypes.Components.Crowds;
using P2XMLEditor.GameData.Templates.InternalTypes.Components.Interactions;
using P2XMLEditor.GameData.Templates.InternalTypes.Components.Player;
using P2XMLEditor.GameData.Templates.InternalTypes.Interfaces;
using P2XMLEditor.Logging;
using static P2XMLEditor.Helper.XmlParsingHelper;

namespace P2XMLEditor.GameData.Templates;

public class Entity : TemplateObject, IEnableable {
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
					try {
						component.LoadFromXml(componentElement);
						Components.Add(component);
					}
					catch (Exception e) {
						Logger.Log(LogLevel.Error, $"Error loading {component.GetType()} from XML: {e}");
					}
				} else {
					if (!invalidComponent.TryAdd(type, 1))
						invalidComponent[type]++;
				}
			}
		}

		IsEnabled = ParseBool(element.Element("IsEnabled")!);
	}
	
	public static ITemplateComponent? CreateComponent(string? type) => type switch {
		// CROWDS
		nameof(IndoorCrowdComponent) => new IndoorCrowdComponent(),
		nameof(OutdoorCrowdComponent) => new OutdoorCrowdComponent(),
		
		// INTERACTIONS
		nameof(FastTravelComponent) => new FastTravelComponent(),
		nameof(MarketComponent) => new MarketComponent(),
		nameof(RepairerComponent) => new RepairerComponent(),
		
		// PLAYER
		nameof(AttackerPlayerComponent) => new AttackerPlayerComponent(),
		nameof(ControllerComponent) => new ControllerComponent(),
		nameof(PlayerControllerComponent) => new PlayerControllerComponent(),
		nameof(PlayerLocationComponent) => new PlayerLocationComponent(),
		nameof(PlayerInteractableComponent) => new PlayerInteractableComponent(),
		
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
		nameof(DetectableComponent) => new DetectableComponent(),
		nameof(TestComponent) => new TestComponent(),
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
		nameof(NpcControllerComponent) => new NpcControllerComponent(),
		nameof(CrowdItemComponent) => new CrowdItemComponent(),
		nameof(SpawnpointComponent) => new SpawnpointComponent(),
		nameof(BoundCharacterComponent) => new BoundCharacterComponent(),
		nameof(StorageComponent) => new StorageComponent(),
		nameof(SelectorComponent) => new SelectorComponent(),
		nameof(MessangerStationaryComponent) => new MessangerStationaryComponent(),
		nameof(CollectControllerComponent) => new CollectControllerComponent(),
		nameof(InventoryComponent) => new InventoryComponent(),
		nameof(MapCustomMarkerComponent) => new MapCustomMarkerComponent(),
		nameof(DynamicModelComponent) => new DynamicModelComponent(),
		nameof(MessangerComponent) => new MessangerComponent(),
		nameof(ParametersComponent) => new ParametersComponent(),
		_ => null
	};

	public XElement ToXml() {
		var element = base.ToXml();

		var componentsElement = new XElement("Components");
		foreach (var component in Components) 
			componentsElement.Add(component.ToXml(new XElement("Item", new XAttribute("type", GetType().Name))));

		element.Add(componentsElement);
		element.Add(CreateBoolElement("IsEnabled", IsEnabled));

		return element;
	}
}