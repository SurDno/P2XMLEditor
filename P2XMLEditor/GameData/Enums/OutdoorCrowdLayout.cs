using System.ComponentModel;
using P2XMLEditor.Attributes;

namespace P2XMLEditor.GameData.Enums;

[TypeConverter(typeof(EnumConverter)), SerializationEnum]
public enum OutdoorCrowdLayout {
	[SerializationData("None")] None,
	[SerializationData("Day_0")] Day0,
	[SerializationData("Day_1")] Day1,
	[SerializationData("Day_2")] Day2,
	[SerializationData("Day_3")] Day3,
	[SerializationData("Day_4")] Day4,
	[SerializationData("Day_5")] Day5,
	[SerializationData("Day_6")] Day6,
	[SerializationData("Day_7")] Day7,
	[SerializationData("Day_8")] Day8,
	[SerializationData("Day_9")] Day9,
	[SerializationData("Day_10")] Day10,
	[SerializationData("Day_11")] Day11,
	[SerializationData("Day_12")] Day12,
	[SerializationData("__Quests")] Quests,
	[SerializationData("EpicQuest_12")] EpicQuest12,
	[SerializationData("Day_3_TheWalk")] Day3TheWalk,
	[SerializationData("__MarbleNest")] MarbleNest,
	[SerializationData("Day_MN")] DayMN,
}