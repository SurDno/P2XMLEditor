using System.ComponentModel;
using P2XMLEditor.Attributes;

namespace P2XMLEditor.GameData.Enums;

[TypeConverter(typeof(EnumConverter)), SerializationEnum]
public enum NpcAttackKind {
	[SerializationData("FrontPunch")] FrontPunch = 1,
	[SerializationData("FrontDodgeCounterPunch")] FrontDodgeCounterPunch = 2,
	[SerializationData("FrontPush")] FrontPush = 3,
	[SerializationData("FrontPunchBlocked")] FrontPunchBlocked = 4,
	[SerializationData("FrontPunchBlockPassed")] FrontPunchBlockPassed = 5
}
