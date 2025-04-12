using System.ComponentModel;
using P2XMLEditor.Attributes;

namespace P2XMLEditor.GameData.Templates.Enums;

[TypeConverter(typeof(EnumConverter))]
public enum BuildingType
{
	[SerializationData("None")] None,
	[SerializationData("Government")] Government,
	[SerializationData("Theater")] Theater,
	[SerializationData("Georgy")] Georgy,
	[SerializationData("Maria")] Maria,
	[SerializationData("Victor")] Victor,
	[SerializationData("Eva")] Eva,
	[SerializationData("Julia")] Julia,
	[SerializationData("Young_Vlad")] YoungVlad,
	[SerializationData("Kapella")] Capella,
	[SerializationData("Olgimski")] Olgimski,
	[SerializationData("Lara")] Lara,
	[SerializationData("Grief")] Grief,
	[SerializationData("Notkin")] Notkin,
	[SerializationData("Mishka")] Murky,
	[SerializationData("Saburov")] Saburov,
	[SerializationData("Katerina")] Katerina,
	[SerializationData("Laska")] Laska,
	[SerializationData("Ospina")] Aspity,
	[SerializationData("Rubin")] Rubin,
	[SerializationData("Anna")] Anna,
	[SerializationData("Haruspex")] Haruspex,
	[SerializationData("Isidor")] Isidor,
	[SerializationData("Polyhedron")] Polyhedron,
	[SerializationData("Cathedral")] Cathedral,
	[SerializationData("Boiny")] Abbatoir,
	[SerializationData("Termitnik")] Termitary,
	[SerializationData("AndrewBar")] AndrewBar,
	[SerializationData("Rubin_Flat")] RubinFlat,
	[SerializationData("Peter_Flat")] PeterFlat,
	[SerializationData("Ospina_MN")] AspityMarbleNest,
	[SerializationData("Bachelor_MN")] BachelorMarbleNest,
	[SerializationData("Houses")] Houses,
	[SerializationData("House_1")] House1,
	[SerializationData("House_2")] House2,
	[SerializationData("House_3")] House3,
	[SerializationData("House_4")] House4,
	[SerializationData("House_5")] House5,
	[SerializationData("House_6")] House6,
	[SerializationData("House_7")] House7
}