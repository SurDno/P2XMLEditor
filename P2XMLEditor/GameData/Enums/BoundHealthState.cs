using System.ComponentModel;
using P2XMLEditor.Attributes;

namespace P2XMLEditor.GameData.Enums;

[TypeConverter(typeof(EnumConverter))]
[SerializationEnum]
public enum BoundHealthState {
	[SerializationData("None")] None,
	[SerializationData("Normal")] Normal,
	[SerializationData("Danger")] Danger,
	[SerializationData("Diseased")] Diseased,
	[SerializationData("Dead")] Dead,
	[SerializationData("__ServiceStates")] ServiceStates,
	[SerializationData("TutorialPain")] TutorialPain,
	[SerializationData("TutorialDiagnostics")] TutorialDiagnostics
}