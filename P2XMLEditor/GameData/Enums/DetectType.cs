using System.ComponentModel;
using P2XMLEditor.Attributes;

namespace P2XMLEditor.GameData.Enums;

[TypeConverter(typeof(EnumConverter))]
[SerializationEnum]
public enum DetectType {
	[SerializationData("None")] None,
	[SerializationData("Casual")] Casual,
	[SerializationData("Wary")] Wary,
	[SerializationData("Aggresive")] Aggressive,
}