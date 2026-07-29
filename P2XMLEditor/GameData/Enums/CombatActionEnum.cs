using System.ComponentModel;
using P2XMLEditor.Attributes;

namespace P2XMLEditor.GameData.Enums;

[TypeConverter(typeof(EnumConverter)), SerializationEnum]
public enum CombatActionEnum {
	[SerializationData("None")] None,
	[SerializationData("Death")] Death,
	[SerializationData("EnterCombat")] EnterCombat,
	[SerializationData("Surrender")] Surrender,
}