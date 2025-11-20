using System.ComponentModel;
using P2XMLEditor.Attributes;

namespace P2XMLEditor.GameData.Enums;

[TypeConverter(typeof(EnumConverter))]
[SerializationEnum]
public enum BlockType {
	[SerializationData("None")] None,
	[SerializationData("NotBlocking")] NotBlocking,
	[SerializationData("Block")] Block,
	[SerializationData("Dodge")] Dodge,
	[SerializationData("Stagger")] Stagger,
	[SerializationData("Surrender")] Surrender
}