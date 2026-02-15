using System.ComponentModel;

namespace P2XMLEditor.Enums.Templates;

[TypeConverter(typeof(EnumConverter)), SerializationEnum]
public enum StammKind  {
	[SerializationData("Unknown")] Unknown,
	[SerializationData("Grey")] Grey,
	[SerializationData("Yellow")] Yellow,
	[SerializationData("Red")] Red
}