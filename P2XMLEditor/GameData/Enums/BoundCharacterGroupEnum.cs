using System.ComponentModel;
using P2XMLEditor.Attributes;

namespace P2XMLEditor.GameData.Enums;

[TypeConverter(typeof(EnumConverter)), SerializationEnum]
public enum BoundCharacterGroupEnum {
	[SerializationData("None")] None,
	[SerializationData("Blood")] Blood,
	[SerializationData("Bones")] Bones,
	[SerializationData("Nerves")] Nerves,
	[SerializationData("List")] List,
	[SerializationData("Earth")] Earth,
	[SerializationData("Queens")] Queens,
	[SerializationData("Pieces")] Pieces,
	[SerializationData("Pawns")] Pawns,
}