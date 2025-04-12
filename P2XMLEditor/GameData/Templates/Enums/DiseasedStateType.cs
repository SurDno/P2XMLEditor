using System.ComponentModel;
using P2XMLEditor.Attributes;

namespace P2XMLEditor.GameData.Templates.Enums;

[TypeConverter(typeof(EnumConverter))]
public enum DiseasedStateType {
	[SerializationData("None")] None,
	[SerializationData("Normal")] Normal,
	[SerializationData("Diseased")] Diseased,
	[SerializationData("Blocked")] Blocked,
	[SerializationData("Shelter")] Shelter
}