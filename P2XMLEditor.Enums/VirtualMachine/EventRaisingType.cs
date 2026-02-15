using System.ComponentModel;

namespace P2XMLEditor.Enums.VirtualMachine;

[TypeConverter(typeof(EnumConverter)), SerializationEnum]
public enum EventRaisingType {
	[SerializationData("EVENT_RAISING_TYPE_BY_ENGINE")] ByEngine,
	[SerializationData("EVENT_RAISING_TYPE_TIME")] Time,
	[SerializationData("EVENT_RAISING_TYPE_CONDITION")] Condition,
	[SerializationData("EVENT_RAISING_TYPE_PARAM_CHANGE")] ParamChange
}