using System.ComponentModel;

namespace P2XMLEditor.Enums.Templates;

[TypeConverter(typeof(EnumConverter)), SerializationEnum]
public enum FastTravelPoint {
	[SerializationData("None")] None,
	[SerializationData("Most")] BridgeSquare,
	[SerializationData("Pochka")] Spleen,
	[SerializationData("Utroba")] Gut,
	[SerializationData("Sedlo")] Flank,
	[SerializationData("Rebro")] Chine,
	[SerializationData("Dubilshikov")] Tanners,
	[SerializationData("Kozevenniy")] Skinners,
	[SerializationData("Zavodi")] Factory,
}