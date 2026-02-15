using System.ComponentModel;

namespace P2XMLEditor.Enums.VirtualMachine;

[TypeConverter(typeof(EnumConverter)), SerializationEnum]
public enum ChildContainerType {
	[SerializationData("Childs")] Childs,
	[SerializationData("SimpleChilds")] SimpleChilds,
	[SerializationData("Scenes")] Scenes
}