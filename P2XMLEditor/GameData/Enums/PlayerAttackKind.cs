using System.ComponentModel;
using P2XMLEditor.Attributes;

namespace P2XMLEditor.GameData.Enums;

[TypeConverter(typeof(EnumConverter)), SerializationEnum]
public enum PlayerAttackKind {
	[SerializationData("FrontPunch")] FrontPunch = 1,
	[SerializationData("FrontDodgeCounterPunch")] FrontDodgeCounterPunch = 2,
	[SerializationData("FrontPush")] FrontPush = 3,
	[SerializationData("FrontDodge")] FrontDodge = 4
}
