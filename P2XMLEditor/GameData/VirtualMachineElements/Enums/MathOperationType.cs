using System.ComponentModel;
using P2XMLEditor.Attributes;

namespace P2XMLEditor.GameData.VirtualMachineElements.Enums;

[TypeConverter(typeof(EnumConverter))]
[SerializationEnum]
public enum MathOperationType {
	[SerializationData("ACTION_OPERATION_TYPE_NONE")] None,
	[SerializationData("ACTION_OPERATION_TYPE_ADDICTION")] Addition,
	[SerializationData("ACTION_OPERATION_TYPE_SUBTRACTION")] Subtraction,
	[SerializationData("ACTION_OPERATION_TYPE_MULTIPLY")] Multiply,
	[SerializationData("ACTION_OPERATION_TYPE_DIVISION")] Division
}