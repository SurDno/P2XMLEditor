using System.ComponentModel;

namespace P2XMLEditor.Enums.Templates;

[TypeConverter(typeof(EnumConverter)), SerializationEnum]
public enum RegionBehaviourEnum {
	[SerializationData("None")] None,
	[SerializationData("AlwaysMaxReputation")] AlwaysMaxReputation,
	[SerializationData("AlwaysMinReputation")] AlwaysMinReputation
}