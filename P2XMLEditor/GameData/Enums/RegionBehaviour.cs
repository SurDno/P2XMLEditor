using System.ComponentModel;
using P2XMLEditor.Attributes;

namespace P2XMLEditor.GameData.Enums;

[TypeConverter(typeof(EnumConverter))]
[SerializationEnum]
public enum RegionBehaviourEnum {
	[SerializationData("None")] None,
	[SerializationData("AlwaysMaxReputation")] AlwaysMaxReputation,
	[SerializationData("AlwaysMinReputation")] AlwaysMinReputation
}