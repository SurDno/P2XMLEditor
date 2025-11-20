using System.ComponentModel;
using P2XMLEditor.Attributes;

namespace P2XMLEditor.GameData.Enums;

[TypeConverter(typeof(EnumConverter))]
[SerializationEnum]
public enum LockState {
	[SerializationData("Unlocked")] Unlocked,
	[SerializationData("Locked")] Locked,
	[SerializationData("Blocked")] Blocked,
}