using System.ComponentModel;
using P2XMLEditor.Attributes;

namespace P2XMLEditor.GameData.Enums;

[TypeConverter(typeof(EnumConverter)), SerializationEnum]
public enum DialogStringEnum {
	[SerializationData("None")] None,
	[SerializationData("Final")] Final
}
