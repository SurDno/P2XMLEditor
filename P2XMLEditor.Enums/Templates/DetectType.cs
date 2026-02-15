using System.ComponentModel;

namespace P2XMLEditor.Enums.Templates;

[TypeConverter(typeof(EnumConverter)), SerializationEnum]
public enum DetectType {
	[SerializationData("None")] None,
	[SerializationData("Casual")] Casual,
	[SerializationData("Wary")] Wary,
	[SerializationData("Aggresive")] Aggressive,
}