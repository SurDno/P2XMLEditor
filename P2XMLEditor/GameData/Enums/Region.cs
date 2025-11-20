using System.ComponentModel;
using P2XMLEditor.Attributes;

namespace P2XMLEditor.GameData.Enums;


[TypeConverter(typeof(EnumConverter))]
[SerializationEnum]
public enum RegionEnum {
	[SerializationData("None")] None,
	[SerializationData("Steppe")] Steppe,
	[SerializationData("Mnogogrannik")] Polyhedron,
	[SerializationData("Stvorki")] Atrium,
	[SerializationData("Most")] BridgeSquare,
	[SerializationData("Mys")] Cape,
	[SerializationData("Serdechnik")] Marrow,
	[SerializationData("Pochka")] Spleen,
	[SerializationData("Hrebtovka")] Backbone,
	[SerializationData("Utroba")] Gut,
	[SerializationData("Skladi")] Warehouses,
	[SerializationData("Sedlo")] Flank,
	[SerializationData("Jerlo")] Maw,
	[SerializationData("Rebro")] Chine,
	[SerializationData("Railroad_Station")] RailroadStation,
	[SerializationData("Dubilshikov")] Tanners,
	[SerializationData("Jilniki")] Hindquarters,
	[SerializationData("Kladbishe")] Cemetery,
	[SerializationData("Kozevenniy")] Skinners,
	[SerializationData("Sirie_Zastroiki")] CrudeSprawl,
	[SerializationData("Zavodi")] Factory,
	[SerializationData("Boyni")] Abbatoir,
	[SerializationData("Hollywood")] Hollywood,
	[SerializationData("Odong_Village")] Shekhen
}