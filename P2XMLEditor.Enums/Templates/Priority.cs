using System.ComponentModel;

namespace P2XMLEditor.Enums.Templates;

[TypeConverter(typeof(EnumConverter)), SerializationEnum]
public enum Priority {
	[SerializationData("None")] None = 0,
	[SerializationData("Default")] Default = 100,
	[SerializationData("RandomQuest")] RandomQuest = 200,
	[SerializationData("Quest")] Quest = 300,
}