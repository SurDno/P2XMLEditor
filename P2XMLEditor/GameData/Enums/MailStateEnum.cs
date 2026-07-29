using System.ComponentModel;
using P2XMLEditor.Attributes;

namespace P2XMLEditor.GameData.Enums;

[TypeConverter(typeof(EnumConverter)), SerializationEnum]
public enum MailStateEnum {
	[SerializationData("None")] None,
	[SerializationData("Available")] Available,
	[SerializationData("Readed")] Readed
}
