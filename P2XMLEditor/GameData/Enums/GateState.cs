using System.ComponentModel;
using P2XMLEditor.Attributes;

namespace P2XMLEditor.GameData.Enums;

[TypeConverter(typeof(EnumConverter))]
[SerializationEnum]
public enum GateState {
	[SerializationData("None")] None,
	[SerializationData("Opened")] Open,
	[SerializationData("Closed")] Closed
}
