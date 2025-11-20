using System.ComponentModel;
using P2XMLEditor.Attributes;

namespace P2XMLEditor.GameData.Enums;

[TypeConverter(typeof(EnumConverter))]
[SerializationEnum]
public enum ActionEnum {
	[SerializationData("None")] None,
	[SerializationData("Theft")] Theft,
	[SerializationData("BreakPicklock")] BreakLockpick,
	[SerializationData("BreakContainer")] BreakContainer,
	[SerializationData("LootDeadCharacter")] LootDeadCharacter,
	[SerializationData("CollectItem")] CollectItem,
	[SerializationData("EnterWithoutKnock")] EnterWithoutKnock,
	[SerializationData("Autopsy")] Autopsy,
	[SerializationData("ShootNpc")] ShootNpc,
	[SerializationData("ShootThugNpc")] ShootThugNpc,
	[SerializationData("MurderNpc")] MurderNpc,
	[SerializationData("MurderThugNpc")] MurderThugNpc,
	[SerializationData("SafeNpc")] SafeNpc,
	[SerializationData("HitNpc")] HitNpc,
	[SerializationData("HitThugNpc")] HitThugNpc,
	[SerializationData("PacifiedTheAgony")] PacifiedTheAgony,
	[SerializationData("SeeInfected")] SeeInfected,
	[SerializationData("TakeItemsFromSurrender")] TakeItemsFromSurrender,
	[SerializationData("HitAnotherNPC")] HitAnotherNPC,
	[SerializationData("FirstAttackNPC")] FirstAttackNPC,
	[SerializationData("FirstAttackThugNPC")] FirstAttackThugNPC,
	[SerializationData("KillDyingNpc")] KillDyingNpc,
	[SerializationData("EutanizeDyingNpc")] EuthanizeDyingNpc,
	[SerializationData("HealNpcPain")] HealNpcPain,
	[SerializationData("HealNpcInfection")] HealNpcInfection,
	[SerializationData("HitAnotherGoodNPC")] HitAnotherGoodNPC,
	[SerializationData("CureInfection")] CureInfection,
	[SerializationData("GiftNPC")] GiftNPC,
	[SerializationData("RepairHydrant")] RepairHydrant,
}