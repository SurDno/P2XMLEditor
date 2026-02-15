using System.ComponentModel;

namespace P2XMLEditor.Enums.Templates;

[TypeConverter(typeof(EnumConverter)), SerializationEnum]
public enum BlockType {
	[SerializationData("None")] None,
	[SerializationData("NotBlocking")] NotBlocking,
	[SerializationData("Block")] Block,
	[SerializationData("Dodge")] Dodge,
	[SerializationData("Stagger")] Stagger,
	[SerializationData("Surrender")] Surrender
}