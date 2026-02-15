using System.ComponentModel;

namespace P2XMLEditor.Enums.Templates;

[TypeConverter(typeof(EnumConverter)), SerializationEnum]
public enum DiseasedStateType {
	[SerializationData("None")] None,
	[SerializationData("Normal")] Normal,
	[SerializationData("Diseased")] Diseased,
	[SerializationData("Blocked")] Burnt,
	[SerializationData("Shelter")] Shelter
}