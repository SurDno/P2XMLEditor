using System.ComponentModel;
using P2XMLEditor.Attributes;

namespace P2XMLEditor.GameData.VirtualMachineElements.Enums;

[TypeConverter(typeof(EnumConverter))]
[SerializationEnum]
public enum ConditionOperation {
	[SerializationData("COP_ROOT")] Root,
	[SerializationData("COP_OR")] Or,
	[SerializationData("COP_AND")] And,
	[SerializationData("COP_XOR")] Xor
}