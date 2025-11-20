using System.ComponentModel;
using P2XMLEditor.Attributes;

namespace P2XMLEditor.GameData.VirtualMachineElements.Enums;

[TypeConverter(typeof(EnumConverter))]
[SerializationEnum]
public enum FormulaOperation {
	[SerializationData("FORMULA_OP_NONE")] None,
	[SerializationData("FORMULA_OP_PLUS")] Plus,
	[SerializationData("FORMULA_OP_MINUS")] Minus,
	[SerializationData("FORMULA_OP_MULTIPLY")] Multiply,
	[SerializationData("FORMULA_OP_DIVIDE")] Divide,
	[SerializationData("FORMULA_OP_RDIVIDE")] RDivide,
	[SerializationData("FORMULA_OP_POWER")] Power,
	[SerializationData("FORMULA_OP_LOG")] Log,
	[SerializationData("FORMULA_OP_LOG10")] Log10,
	[SerializationData("FORMULA_OP_EXP")] Exp,
	[SerializationData("FORMULA_OP_SIN")] Sin,
	[SerializationData("FORMULA_OP_COS")] Cos
}