using System.ComponentModel;
using P2XMLEditor.Attributes;

namespace P2XMLEditor.GameData.Enums;

[TypeConverter(typeof(EnumConverter))]
[SerializationEnum]
public enum SpawnpointKind {
	[SerializationData("None")] None,
	[SerializationData("Open")] Open,
	[SerializationData("Hidden")] Hidden,
	[SerializationData("Open2")] Open2,
	[SerializationData("OpenPaired1")] OpenPaired1,
	[SerializationData("OpenPaired2")] OpenPaired2,
	[SerializationData("Observer")] Observer,
	[SerializationData("TragediansWalk")] TragediansWalk,
}