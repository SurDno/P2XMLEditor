using System.ComponentModel;
using P2XMLEditor.Attributes;

namespace P2XMLEditor.GameData.Enums;

[TypeConverter(typeof(EnumConverter))]
[SerializationEnum]
public enum CombatStyle {
	[SerializationData("None")] None,
	[SerializationData("Default")] Default,
	[SerializationData("Berserker")] Berserker,
	[SerializationData("Agile")] Agile,
	[SerializationData("Coward")] Coward,
	[SerializationData("Victim")] Victim,
	[SerializationData("Soldier")] Soldier,
	[SerializationData("Bomber")] Bomber,
	[SerializationData("Diseased")] Diseased,
	[SerializationData("Sanitar")] Sanitar,
	[SerializationData("Exhausting")] Exhausting,
	[SerializationData("Child")] Child,
	[SerializationData("Woman")] Woman,
	[SerializationData("Patrolman")] Patrolman,
	[SerializationData("Bandit")] Bandit,
	[SerializationData("Odong")] Odong,
	[SerializationData("VictimLastDays")] VictimLastDays,
	[SerializationData("OdongLastDays")] OdongLastDays,
}