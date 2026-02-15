using System.ComponentModel;

namespace P2XMLEditor.Enums.VirtualMachine;

[TypeConverter(typeof(EnumConverter)), SerializationEnum]
public enum ActionType {
	[SerializationData("ACTION_TYPE_NONE")] None,
	[SerializationData("ACTION_TYPE_SET_PARAM")] SetParam,
	[SerializationData("ACTION_TYPE_SET_EXPRESSION")] SetExpression,
	[SerializationData("ACTION_TYPE_MATH")] Math,
	[SerializationData("ACTION_TYPE_DO_FUNCTION")] DoFunction,
	[SerializationData("ACTION_TYPE_RAISE_EVENT")] RaiseEvent
}