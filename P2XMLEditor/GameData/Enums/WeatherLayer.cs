using System.ComponentModel;
using P2XMLEditor.Attributes;

namespace P2XMLEditor.GameData.Enums;

[SerializationEnum]
public enum WeatherLayer {
	[SerializationData("BaseLayer")] BaseLayer,
	[SerializationData("PlannedEventsLayer")] PlannedEventsLayer,
	[SerializationData("DistrictLayer")] DistrictLayer,
	[SerializationData("CutSceneLayer")] CutSceneLayer
}
