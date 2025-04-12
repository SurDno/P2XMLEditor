using System.ComponentModel;
using P2XMLEditor.Attributes;

namespace P2XMLEditor.GameData.Templates.Enums;

[TypeConverter(typeof(EnumConverter))]
public enum EntityTag {
	[SerializationData("None")] None,
	[SerializationData("DiseasedLevels")] DiseasedLevels,
	[SerializationData("DiseasedLevel0")] DiseasedLevel0,
	[SerializationData("DiseasedLevel1")] DiseasedLevel1,
	[SerializationData("DiseasedLevel2")] DiseasedLevel2,
	[SerializationData("DiseasedLevel3")] DiseasedLevel3,
	[SerializationData("DiseasedLevel4")] DiseasedLevel4,
	[SerializationData("DiseasedLevel5")] DiseasedLevel5,
	[SerializationData("DiseasedScene")] DiseasedScene,
	[SerializationData("DiseasedIndoorCorpses")] DiseasedIndoorCorpses,
	[SerializationData("Event_Phantom_ObjectsEnabled")] EventPhantomObjectsEnabled,
	[SerializationData("Event_Phantom_ObjectsDisabled")] EventPhantomObjectsDisabled,
	[SerializationData("Event_Phantom_OpenedDoor")] EventPhantomOpenedDoor,
	[SerializationData("Event_Phantom_SpawnNPC")] EventPhantomSpawnNpc,
	[SerializationData("Event_Phantom_SpawnNPCSoundOutdoor")] EventPhantomSpawnNpcSoundOutdoor,
	[SerializationData("Event_Phantom_SpawnNPCSoundIndoor")] EventPhantomSpawnNpcSoundIndoor,
	[SerializationData("Event_Marooned_ObjectsEnabled")] EventMaroonedObjectsEnabled,
	[SerializationData("Event_Marooned_ObjectsDisabled")] EventMaroonedObjectsDisabled,
	[SerializationData("Event_Marooned_SpawnKaterina")] EventMaroonedSpawnKaterina,
	[SerializationData("Event_TheWalk_ObjectsEnabled")] EventTheWalkObjectsEnabled,
	[SerializationData("Event_TheWalk_ObjectsDisabled")] EventTheWalkObjectsDisabled,
	[SerializationData("Event_Town_Reflections_ObjectsEnabled")] EventTownReflectionsObjectsEnabled,
	[SerializationData("Event_Town_Reflections_ObjectsDisabled")] EventTownReflectionsObjectsDisabled,
	[SerializationData("Event_Town_Reflections_SpawnNPC")] EventTownReflectionsSpawnNpc,
	[SerializationData("Event_Town_Reflections_AdultsCrowd")] EventTownReflectionsAdultsCrowd,
	[SerializationData("Event_Town_Reflections_KidsCrowd")] EventTownReflectionsKidsCrowd,
	[SerializationData("Water_Supply_Hydrant")] WaterSupplyHydrant,
	[SerializationData("Water_Supply_Barrel")] WaterSupplyBarrel,
	[SerializationData("Event_BadEnding_ObjectsEnabled")] EventBadEndingObjectsEnabled,
	[SerializationData("Event_BadEnding_ObjectsDisabled")] EventBadEndingObjectsDisabled
}