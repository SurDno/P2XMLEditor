using System.ComponentModel;
using P2XMLEditor.Attributes;

namespace P2XMLEditor.GameData.Enums;

[TypeConverter(typeof(EnumConverter))]
[SerializationEnum]
public enum GameLocalizationName {
	[SerializationData("english")] english,
	[SerializationData("russian")] russian,
	[SerializationData("german")] german,
	[SerializationData("japanese")] japanese,
	[SerializationData("french")] french,
}
