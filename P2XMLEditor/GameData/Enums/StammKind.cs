using System.ComponentModel;
using P2XMLEditor.Attributes;

namespace P2XMLEditor.GameData.Enums;

[TypeConverter(typeof(EnumConverter))]
[SerializationEnum]
public enum StammKind  {
	[SerializationData("Unknown")] Unknown,
	[SerializationData("Grey")] Grey,
	[SerializationData("Yellow")] Yellow,
	[SerializationData("Red")] Red
}