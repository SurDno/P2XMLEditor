using System.ComponentModel;
using P2XMLEditor.Attributes;

namespace P2XMLEditor.GameData.VirtualMachineElements.Enums;

[TypeConverter(typeof(EnumConverter))]
[SerializationEnum]
public enum ChildContainerType {
	[SerializationData("Childs")] Childs,
	[SerializationData("SimpleChilds")] SimpleChilds,
	[SerializationData("Scenes")] Scenes
}