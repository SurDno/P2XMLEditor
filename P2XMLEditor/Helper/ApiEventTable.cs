using System.Collections.Generic;

namespace P2XMLEditor.GameData.VirtualMachineElements.Helper;

public static class ApiEventTable {
	public readonly record struct ApiMessage(string ParamName, string Type);

	public readonly record struct ApiEvent(string SpecialName, IReadOnlyList<ApiMessage> Messages);

	public static string MessageName(string eventName, string paramName) =>
		$"{eventName}_message_{paramName}";

	public static bool TryGet(string component, string eventName, out ApiEvent result) =>
		Events.TryGetValue($"{component}.{eventName}", out result);

	public static ApiEvent Get(string component, string eventName) => Events[$"{component}.{eventName}"];
	
	public static readonly IReadOnlyDictionary<string, ApiEvent> Events =
		new Dictionary<string, ApiEvent> {
			["AttackerPlayer.HandsHolsteredEvent"] = new(null, [new("weapon type", "WeaponKind")]),
			["AttackerPlayer.HandsUnholsteredEvent"] = new(null, [new("weapon type", "WeaponKind")]),
			["BehaviorComponent.Custom"] = new(null, [new("value", "System.String")]),
			["BehaviorComponent.Fail"] = new(null, []),
			["BehaviorComponent.Success"] = new(null, []),
			["BlueprintComponent.Attach"] = new(null, []),
			["BlueprintComponent.Complete"] = new(null, []),
			["BoundCharacterComponent.OnChangeBoundHealthState"] = new(null, [new("Value", "BoundHealthStateEnum")]),
			["Common.RemoveEvent"] = new(null, []),
			["Common.StartEvent"] = new("SEN_START_OBJECT_FSM", []),
			["Controller.BeginControllIteractEvent"] = new(null, [new("агент", "IObjRef%cf_Controller"), new("цель", "IObjRef%cf_Interactive"), new("тип", "InteractType")]),
			["Controller.EndControllIteractEvent"] = new(null, [new("агент", "IObjRef%cf_Controller"), new("цель", "IObjRef%cf_Interactive"), new("тип", "InteractType")]),
			["Detector.OnHear"] = new(null, [new("detected object", "IObjRef%cf_Detectable")]),
			["Detector.OnSee"] = new(null, [new("detected object", "IObjRef%cf_Detectable")]),
			["Detector.OnStopSee"] = new(null, [new("detected object", "IObjRef%cf_Detectable")]),
			["FastTravelComponent.TravelToPoint"] = new(null, [new("Travel target", "FastTravelPointEnum"), new(" Travel time", "GameTime")]),
			["GameComponent.NeedCreateDropBagEvent"] = new(null, [new("template object", "IBlueprintRef")]),
			["GameComponent.NeedDeleteDropBagEvent"] = new(null, [new("object", "IObjRef")]),
			["GameComponent.OnCommonConsoleEvent"] = new(null, [new("value", "System.String")]),
			["GameComponent.OnEndCutsceneEvent"] = new(null, []),
			["GameComponent.OnEndStateSleep"] = new("SEN_ON_LOCAL_TIMER", [new("stateId", "System.UInt64")]),
			["GameComponent.OnEntityLogicEvent"] = new(null, [new("name", "System.String"), new("entity", "IObjRef")]),
			["GameComponent.OnFurnitureLoaded"] = new(null, [new("Entity", "IObjRef"), new(" Region", "IObjRef%cf_Region"), new(" BuildingEnum", "BuildingEnum"), new(" DiseasedStateEnum", "DiseasedStateEnum")]),
			["GameComponent.OnFurnitureLoadedOnce"] = new(null, [new("Entity", "IObjRef"), new(" Region", "IObjRef%cf_Region"), new(" BuildingEnum", "BuildingEnum"), new(" DiseasedStateEnum", "DiseasedStateEnum")]),
			["GameComponent.OnGameModeChanged"] = new(null, []),
			["GameComponent.OnLoadGame"] = new("SEN_LOAD_GAME", []),
			["GameComponent.OnRegionDiseaseLevelChangedEvent"] = new(null, [new("Region", "IObjRef%cf_Region"), new(" level of disease", "System.Int32")]),
			["GameComponent.OnRegionLoaded"] = new(null, [new("Region", "IObjRef%cf_Region")]),
			["GameComponent.OnRegionLoadedOnce"] = new(null, [new("Region", "IObjRef%cf_Region")]),
			["GameComponent.OnRegionReputationChangedEvent"] = new(null, [new("Region", "IObjRef%cf_Region"), new(" reputation", "System.Single")]),
			["GameComponent.OnStartGame"] = new("SEN_START_GAME", []),
			["GameComponent.OnTemplateEntityLogicEvent"] = new(null, [new("name", "System.String"), new("template entity", "IBlueprintRef")]),
			["GameComponent.OnTimer"] = new("SEN_ON_GLOBAL_TIMER", [new("timerId", "System.UInt64")]),
			["GameComponent.OnValueLogicEvent"] = new(null, [new("name", "System.String"), new("value", "System.String")]),
			["HerbRootsComponent.ActivateEndEvent"] = new(null, []),
			["HerbRootsComponent.ActivateStartEvent"] = new(null, []),
			["HerbRootsComponent.HerbSpawnEvent"] = new(null, []),
			["HerbRootsComponent.LastHerbSpawnEvent"] = new(null, []),
			["HerbRootsComponent.TriggerEnterEvent"] = new(null, []),
			["HerbRootsComponent.TriggerLeaveEvent"] = new(null, []),
			["IndoorCrowdComponent.NeedCreateObjectEvent"] = new(null, [new("template object", "IBlueprintRef")]),
			["IndoorCrowdComponent.NeedDeleteObjectEvent"] = new(null, [new("object", "IObjRef")]),
			["Interactive.BeginIteractEvent"] = new(null, [new("агент", "IObjRef%cf_Controller"), new("цель", "IObjRef%cf_Interactive"), new("тип", "InteractType")]),
			["Interactive.EndIteractEvent"] = new(null, [new("агент", "IObjRef%cf_Controller"), new("цель", "IObjRef%cf_Interactive"), new("тип", "InteractType")]),
			["LipSync.PlayCompleteEvent"] = new(null, []),
			["Location.OnHibernationChange"] = new(null, []),
			["Location.OnPlayerInside"] = new(null, [new("is inside", "System.Boolean")]),
			["LocationItem.OnChangeHibernation"] = new(null, []),
			["LocationItem.OnChangeLocation"] = new(null, [new("Location", "IObjRef")]),
			["NpcControllerComponent.ActionEvent"] = new(null, [new("Action type", "Action")]),
			["NpcControllerComponent.ChangeAwayEvent"] = new(null, [new("Value", "System.Boolean")]),
			["NpcControllerComponent.CombatActionEvent"] = new(null, [new("Action type", "CombatAction"), new(" Entity", "IObjRef")]),
			["NpcControllerComponent.OnChangeHealth"] = new(null, [new("Value", "System.Single")]),
			["NpcControllerComponent.OnChangePain"] = new(null, [new("Value", "System.Single")]),
			["OutdoorCrowdComponent.NeedCreateObjectEvent"] = new(null, [new("template object", "IBlueprintRef")]),
			["OutdoorCrowdComponent.NeedDeleteObjectEvent"] = new(null, [new("object", "IObjRef")]),
			["PlayerControllerComponent.CombatActionEvent"] = new(null, [new("Action type", "CombatAction"), new(" Entity", "IObjRef")]),
			["PlayerControllerComponent.OnChangeHealth"] = new(null, [new("Value", "System.Single")]),
			["PlayerControllerComponent.OnChangeInfection"] = new(null, [new("Value", "System.Single")]),
			["PlayerControllerComponent.OnChangePreInfection"] = new(null, [new("Value", "System.Single")]),
			["PlayerControllerComponent.OnChangeSleep"] = new(null, [new("Value", "System.Boolean")]),
			["Position.ArrivedAreaEvent"] = new(null, [new("Тип области", "Area")]),
			["Position.ArrivedBuildingEvent"] = new(null, [new("Building", "IObjRef")]),
			["Position.ArrivedRegionEvent"] = new(null, [new("Регион", "IObjRef%cf_Region")]),
			["Position.LeaveAreaEvent"] = new(null, [new("Тип области", "Area")]),
			["Position.LeaveBuildingEvent"] = new(null, [new("Building", "IObjRef")]),
			["Position.LeaveRegionEvent"] = new(null, [new("Регион", "IObjRef%cf_Region")]),
			["Region.DiseaseLevelChanged"] = new(null, [new("level of disease", "System.Int32")]),
			["Region.ReputationChanged"] = new(null, [new("level of disease", "System.Single")]),
			["RepairableComponent.OnChangeDurability"] = new(null, [new("Value", "System.Single")]),
			["Speaking.BeginTalkingEvent"] = new("SEN_BEGIN_TALKING", []),
			["Speaking.EndTalkingEvent"] = new(null, [new("talking graph", "IStateRef%TalkingGraph")]),
			["Speaking.OnSpeechReplyEvent"] = new("SEN_SPEECH_REPLY", [new("text_guid", "System.UInt64")]),
			["Storage.AddItemEvent"] = new(null, [new("item", "IObjRef%cf_Storable"), new("Container template", "IBlueprintRef%cf_Inventory")]),
			["Storage.ChangeItemEvent"] = new(null, [new("item", "IObjRef%cf_Storable"), new("Container template", "IBlueprintRef%cf_Inventory")]),
			["Storage.RemoveItemEvent"] = new(null, [new("item", "IObjRef%cf_Storable"), new("Container template", "IBlueprintRef%cf_Inventory")]),
			["Trigger.ObjectEnterEvent"] = new(null, [new("Object", "IObjRef")]),
			["Trigger.ObjectExitEvent"] = new(null, [new("Object", "IObjRef")]),
		};
}