using System.ComponentModel;
using P2XMLEditor.Attributes;

namespace P2XMLEditor.GameData.Enums;

[TypeConverter(typeof(EnumConverter)), SerializationEnum]
public enum PlayerPushesDiseasedKind {
	[SerializationData("Unknown")] Unknown,
	[SerializationData("FrontalPush")] FrontalPush,
	[SerializationData("PushToLeft")] PushToLeft,
	[SerializationData("PushToRight")] PushToRight
}
