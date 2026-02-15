using System.ComponentModel;

namespace P2XMLEditor.Enums.Templates;

[TypeConverter(typeof(EnumConverter)), SerializationEnum]
public enum LockState {
	[SerializationData("Unlocked")] Unlocked,
	[SerializationData("Locked")] Locked,
	[SerializationData("Blocked")] Blocked,
}