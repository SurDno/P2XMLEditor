using System.ComponentModel;
using P2XMLEditor.Attributes;

namespace P2XMLEditor.GameData.Enums;

[TypeConverter(typeof(EnumConverter)), SerializationEnum]
public enum OutdoorCrowdLayout {
	[SerializationData("None")] None,
	[SerializationData("Day_3_TheWalk")] Day3TheWalk,
}

[TypeConverter(typeof(EnumConverter)), SerializationEnum]
public enum CombatActionEnum {
	[SerializationData("None")] None,
	[SerializationData("Death")] Death,
	[SerializationData("EnterCombat")] EnterCombat,
	[SerializationData("Surrender")] Surrender,
}

[TypeConverter(typeof(EnumConverter)), SerializationEnum]
public enum JerboaColor {
	[SerializationData("Default")] Default,
}

[TypeConverter(typeof(EnumConverter)), SerializationEnum]
public enum BoundCharacterGroupEnum {
	[SerializationData("None")] None,
	[SerializationData("Blood")] Blood,
	[SerializationData("Bones")] Bones,
	[SerializationData("Nerves")] Nerves,
	[SerializationData("List")] List,
}
