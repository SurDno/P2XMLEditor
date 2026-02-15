using System.ComponentModel;

namespace P2XMLEditor.Enums.Templates;

[TypeConverter(typeof(EnumConverter)), SerializationEnum]
public enum ContainerOpenState {
	[SerializationData("None")] None,
	[SerializationData("Open")] Open,
	[SerializationData("Closed")] Closed,
	[SerializationData("Locked")] Locked,
}
