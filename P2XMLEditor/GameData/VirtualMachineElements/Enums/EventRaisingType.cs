using System.ComponentModel;
using P2XMLEditor.Attributes;

namespace P2XMLEditor.GameData.VirtualMachineElements.Enums;

[TypeConverter(typeof(EnumConverter))]
public enum EventRaisingType {
	[SerializationData("EVENT_RAISING_TYPE_BY_ENGINE")] ByEngine,
	[SerializationData("EVENT_RAISING_TYPE_TIME")] Time,
	[SerializationData("EVENT_RAISING_TYPE_CONDITION")] Condition,
	[SerializationData("EVENT_RAISING_TYPE_PARAM_CHANGE")] ParamChange
}