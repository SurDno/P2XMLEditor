using System.ComponentModel;
using P2XMLEditor.Attributes;

namespace P2XMLEditor.GameData.Enums;

[TypeConverter(typeof(EnumConverter)), SerializationEnum]
public enum JerboaColor {
	[SerializationData("None")] None,
	[SerializationData("Default")] Default,
	[SerializationData("Black")] Black,
}