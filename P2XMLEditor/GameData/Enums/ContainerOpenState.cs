using System.ComponentModel;
using P2XMLEditor.Attributes;

namespace P2XMLEditor.GameData.Enums;

[TypeConverter(typeof(EnumConverter))]
[SerializationEnum]
public enum ContainerOpenState {
	[SerializationData("None")] None,
	[SerializationData("Open")] Open,
	[SerializationData("Closed")] Closed,
	[SerializationData("Locked")] Locked,
}
