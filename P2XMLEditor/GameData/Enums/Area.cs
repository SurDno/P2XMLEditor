using System.ComponentModel;
using P2XMLEditor.Attributes;

namespace P2XMLEditor.GameData.Enums;

[TypeConverter(typeof(EnumConverter))]
[SerializationEnum]
public enum Area {
	[SerializationData("Unknown")] Unknown = 0, // Group="NavMesh"
	[SerializationData("Walkable")] Walkable = 1,
	[SerializationData("NotWalkable")] NotWalkable = 2,
	[SerializationData("Jump")] Jump = 3,
	[SerializationData("Indoor")] Indoor = 4,
	[SerializationData("Outdoor")] Outdoor = 5,
	[SerializationData("FootPath")] FootPath = 6,
	[SerializationData("Road")] Road = 7,
	[SerializationData("All")] All = 256,
	[SerializationData("RoadFootPath")] RoadFootPath = 257,
	[SerializationData("RoadFootPathWalkable")] RoadFootPathWalkable = 258,
	[SerializationData("__EndMasks")] EndMasks = 512,
	[SerializationData("Agony")] Agony = 513, // Group="Default"
	[SerializationData("Collectable")] Collectable = 514,
	[SerializationData("Family")] Family = 515,
	[SerializationData("Diseased")] Diseased = 516,
	[SerializationData("Sick")] Sick = 517,
	[SerializationData("PlagueCloud")] PlagueCloud = 518,
	[SerializationData("CustomPoints")] CustomPoints = 1024,
	[SerializationData("PlayGround")] PlayGroundGroup = 1025, // Group="PlayGrounds"
	[SerializationData("PlayGround1")] PlayGround1 = 1026,
	[SerializationData("PlayGround2")] PlayGround2 = 1027,
	[SerializationData("PlayGround3")] PlayGround3 = 1028,
	[SerializationData("__Shaika")] ShaikaGroup = 1089, // Group="Shaika"
	[SerializationData("Shaika1")] Shaika1 = 1090,
	[SerializationData("Shaika2")] Shaika2 = 1091,
	[SerializationData("Shaika3")] Shaika3 = 1092,
	[SerializationData("__SickPlace")] SickPlaceGroup = 1153, // Group="SickPlaces"
	[SerializationData("SickPlace1")] SickPlace1 = 1154,
	[SerializationData("SickPlace2")] SickPlace2 = 1155,
	[SerializationData("SickPlace3")] SickPlace3 = 1156,
	[SerializationData("__Bonfires")] BonfiresGroup = 1217, // Group="Bonfires"
	[SerializationData("Bonfires1")] Bonfires1 = 1218,
	[SerializationData("Bonfires2")] Bonfires2 = 1219,
	[SerializationData("Bonfires3")] Bonfires3 = 1220,
	[SerializationData("__CitizensIdles")] CitizensIdlesCategory = 1281, // Category="CitizensIdles"
	[SerializationData("CitizensIdles1")] CitizensIdles1 = 1282,
	[SerializationData("CitizensIdles2")] CitizensIdles2 = 1283,
	[SerializationData("CitizensIdles3")] CitizensIdles3 = 1284,
	[SerializationData("__CitizensWalkable")] CitizensWalkableCategory = 1345, // Category="CitizensWalkable"
	[SerializationData("CitizensWalkable1")] CitizensWalkable1 = 1346,
	[SerializationData("CitizensWalkable2")] CitizensWalkable2 = 1347,
	[SerializationData("CitizensWalkable3")] CitizensWalkable3 = 1348,
	[SerializationData("__MarketPlace")] MarketPlaceCategory = 1409, // Category="MarketPlace"
	[SerializationData("MarketPlace1")] MarketPlace1 = 1410,
	[SerializationData("MarketPlace2")] MarketPlace2 = 1411,
	[SerializationData("MarketPlace3")] MarketPlace3 = 1412,
	[SerializationData("__Family")] FamilyCategory = 1473, // Category="Family"
	[SerializationData("FamilyAdult")] FamilyAdult = 1474,
	[SerializationData("FamilyTeen")] FamilyTeen = 1475,
	[SerializationData("FamilyKid")] FamilyKid = 1476,
	[SerializationData("__Diseased")] DiseasedCategory = 1537, // Category="Diseased"
	[SerializationData("Diseased1")] Diseased1 = 1538,
	[SerializationData("Diseased2")] Diseased2 = 1539,
	[SerializationData("Diseased3")] Diseased3 = 1540,
	[SerializationData("__PlagueCloud")] PlagueCloudCategory = 1601, // Category="PlagueCloud"
	[SerializationData("PlagueCloudStatic")] PlagueCloudStatic = 1602,
	[SerializationData("PlagueCloudAmbush")] PlagueCloudAmbush = 1603,
	[SerializationData("PlagueCloudHunt")] PlagueCloudHunt = 1604,
	[SerializationData("PlagueCloudBullet")] PlagueCloudBullet = 1605,
	[SerializationData("PlagueCloudPatrol")] PlagueCloudPatrol = 1606,
	[SerializationData("__TownPatrol")] TownPatrolCategory = 1665, // Category="TownPatrol"
	[SerializationData("TownPatrolStatic")] TownPatrolStatic = 1666,
	[SerializationData("TownPatrolDynamic")] TownPatrolDynamic = 1667,
	[SerializationData("TownPatrolIndoor")] TownPatrolIndoor = 1668,
	[SerializationData("__Barricades")] BarricadesCategory = 1729, // Category="Barricades"
	[SerializationData("Barricade1")] Barricade1 = 1730,
	[SerializationData("Barricade2")] Barricade2 = 1731,
	[SerializationData("Barricade3")] Barricade3 = 1732,
	[SerializationData("Barricade4")] Barricade4 = 1733,
	[SerializationData("Barricade5")] Barricade5 = 1734,
	[SerializationData("Barricade6")] Barricade6 = 1735,
	[SerializationData("__Patient")] PatientCategory = 1793, // Category="Patient"
	[SerializationData("Patient1")] Patient1 = 1794,
	[SerializationData("Patient2")] Patient2 = 1795,
	[SerializationData("Patient3")] Patient3 = 1796,
	[SerializationData("__Herb")] HerbCategory = 1857, // Category="Herb"
	[SerializationData("Herb1_1")] Herb11 = 1858,
	[SerializationData("Herb1_2")] Herb12 = 1859,
	[SerializationData("Herb2_1")] Herb21 = 1860,
	[SerializationData("Herb2_2")] Herb22 = 1861,
	[SerializationData("Herb3_1")] Herb31 = 1862,
	[SerializationData("Herb3_2")] Herb32 = 1863,
	[SerializationData("Herb_Special")] HerbSpecial = 1864,
	[SerializationData("__Thug")] ThugCategory = 1921, // Category="Thug"
	[SerializationData("Thug1")] Thug1 = 1922,
	[SerializationData("Thug2")] Thug2 = 1923,
	[SerializationData("__Corpse")] CorpseCategory = 1985, // Category="Corpse"
	[SerializationData("Corpse1")] Corpse1 = 1986,
	[SerializationData("Corpse2")] Corpse2 = 1987,
	[SerializationData("__Bride")] BrideCategory = 2049, // Category="Bride"
	[SerializationData("DancingBride")] DancingBride = 2050,
	[SerializationData("__Bull")] BullCategory = 2113, // Category="Bull"
	[SerializationData("Bull_1")] Bull1 = 2114,
	[SerializationData("Bull_2")] Bull2 = 2115,
}