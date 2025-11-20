using System.ComponentModel;
using P2XMLEditor.Attributes;

namespace P2XMLEditor.GameData.Enums;

[TypeConverter(typeof(EnumConverter))]
[SerializationEnum]
public enum DiseasedStateType {
	[SerializationData("None")] None,
	[SerializationData("Normal")] Normal,
	[SerializationData("Diseased")] Diseased,
	[SerializationData("Blocked")] Burnt,
	[SerializationData("Shelter")] Shelter
}