using System.ComponentModel;

namespace P2XMLEditor.Enums.VirtualMachine;

[TypeConverter(typeof(EnumConverter)), SerializationEnum]
public enum MathOperationType {
	[SerializationData("ACTION_OPERATION_TYPE_NONE")] None,
	[SerializationData("ACTION_OPERATION_TYPE_ADDICTION")] Addition,
	[SerializationData("ACTION_OPERATION_TYPE_SUBTRACTION")] Subtraction,
	[SerializationData("ACTION_OPERATION_TYPE_MULTIPLY")] Multiply,
	[SerializationData("ACTION_OPERATION_TYPE_DIVISION")] Division
}