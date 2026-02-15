using System.ComponentModel;

namespace P2XMLEditor.Enums.Templates;

[TypeConverter(typeof(EnumConverter)), SerializationEnum]
public enum Kind {
	[SerializationData("Point")] Point,
	[SerializationData("Box")] Box,
	[SerializationData("Сylinder")] Сylinder,
}