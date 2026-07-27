using System;
using P2XMLEditor.Attributes;

namespace P2XMLEditor.GameData.Enums;

[Flags]
public enum VmComponent : ulong {
	[Component("None")]
	None = 0uL,
	[Component("AttackerPlayer")]
	AttackerPlayer = 1uL,
	[Component("Building")]
	Building = 2uL,
	[Component("Gate")]
	Gate = 4uL,
	[Component("Common")]
	Common = 8uL,
	[Component("Controller")]
	Controller = 0x10uL,
	[Component("HerbRootsComponent")]
	HerbRootsComponent = 0x20uL,
	[Component("InfoParams")]
	InfoParams = 0x40uL,
	[Component("Location")]
	Location = 0x80uL,
	[Component("Milestone")]
	Milestone = 0x100uL,
	[Component("Model")]
	Model = 0x200uL,
	[Component("BehaviorComponent")]
	BehaviorComponent = 0x400uL,
	[Component("BlueprintComponent")]
	BlueprintComponent = 0x800uL,
	[Component("Position")]
	Position = 0x1000uL,
	[Component("Interactive")]
	Interactive = 0x2000uL,
	[Component("Detector")]
	Detector = 0x4000uL,
	[Component("Detectable")]
	Detectable = 0x8000uL,
	[Component("DamageReceiver")]
	DamageReceiver = 0x10000uL,
	[Component("PlayerControllerComponent")]
	PlayerControllerComponent = 0x20000uL,
	[Component("Region")]
	Region = 0x40000uL,
	[Component("Speaking")]
	Speaking = 0x80000uL,
	[Component("LocationItem")]
	LocationItem = 0x100000uL,
	[Component("Storable")]
	Storable = 0x200000uL,
	[Component("Storage")]
	Storage = 0x400000uL,
	[Component("MapItemComponent")]
	MapItemComponent = 0x800000uL,
	[Component("NpcControllerComponent")]
	NpcControllerComponent = 0x1000000uL,
	[Component("Scene")]
	Scene = 0x2000000uL,
	[Component("TagsComponent")]
	TagsComponent = 0x4000000uL,
	[Component("LipSync")]
	LipSync = 0x8000000uL,
	[Component("CrowdItemComponent")]
	CrowdItemComponent = 0x10000000uL,
	[Component("FastTravelComponent")]
	FastTravelComponent = 0x20000000uL,
	[Component("Market")]
	Market = 0x40000000uL,
	[Component("BoundCharacterComponent")]
	BoundCharacterComponent = 0x80000000uL,
	[Component("MessangerComponent")]
	MessangerComponent = 0x100000000uL,
	[Component("MessangerStationaryComponent")]
	MessangerStationaryComponent = 0x200000000uL,
	[Component("Trigger")]
	Trigger = 0x400000000uL,
	[Component("WaterSupplyControllerComponent")]
	WaterSupplyControllerComponent = 0x800000000uL
}
