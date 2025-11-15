using P2XMLEditor.Attributes;

namespace P2XMLEditor.GameData.Templates.Enums;

public enum RegionBehaviourEnum {
	[SerializationData("None")] None,
	[SerializationData("AlwaysMaxReputation")] AlwaysMaxReputation,
	[SerializationData("AlwaysMinReputation")] AlwaysMinReputation
}