using System.ComponentModel;
using P2XMLEditor.Attributes;

namespace P2XMLEditor.GameData.Templates.Enums;

[TypeConverter(typeof(EnumConverter))]
public enum StorableTooltipName {
	[SerializationData("None")] None,
	[SerializationData("Durability")] Durability,
	[SerializationData("Protection")] Protection,
	[SerializationData("Effect")] Effect,
	[SerializationData("Type")] Type,
	[SerializationData("Immunity")] Immunity,
	[SerializationData("Infection")] Infection,
	[SerializationData("Health")] Health,
	[SerializationData("Hunger")] Hunger,
	[SerializationData("Thirst")] Thirst,
	[SerializationData("Fatigue")] Fatigue,
	[SerializationData("Stamina")] Stamina
}