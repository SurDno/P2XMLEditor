using System.ComponentModel;

namespace P2XMLEditor.Enums.VirtualMachine;

[TypeConverter(typeof(EnumConverter)), SerializationEnum]
public enum ConditionOperation {
	[SerializationData("COP_ROOT")] Root,
	[SerializationData("COP_OR")] Or,
	[SerializationData("COP_AND")] And,
	[SerializationData("COP_XOR")] Xor
}