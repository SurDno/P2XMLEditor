using System.ComponentModel;

namespace P2XMLEditor.Enums.Templates;

[TypeConverter(typeof(EnumConverter)), SerializationEnum]
public enum LiquidType {
	[SerializationData("None")] None,
	[SerializationData("Normal")] CleanWater,
	[SerializationData("Muddy")] MuddyWater,
	[SerializationData("AurochsBlood")] AurochsBlood
}