using System.ComponentModel;
using P2XMLEditor.Attributes;

namespace P2XMLEditor.GameData.Enums;

[TypeConverter(typeof(EnumConverter))]
[SerializationEnum]
public enum Kind {
	[SerializationData("Point")] Point,
	[SerializationData("Box")] Box,
	[SerializationData("Сylinder")] Сylinder,
}