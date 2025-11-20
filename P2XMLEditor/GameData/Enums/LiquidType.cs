using System.ComponentModel;
using P2XMLEditor.Attributes;

namespace P2XMLEditor.GameData.Enums;

[TypeConverter(typeof(EnumConverter))]
[SerializationEnum]
public enum LiquidType {
	[SerializationData("None")] None,
	[SerializationData("Normal")] CleanWater,
	[SerializationData("Muddy")] MuddyWater,
	[SerializationData("AurochsBlood")] AurochsBlood
}