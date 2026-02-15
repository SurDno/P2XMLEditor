using System.ComponentModel;

namespace P2XMLEditor.Enums.VirtualMachine;

[TypeConverter(typeof(EnumConverter)), SerializationEnum]
public enum ExpressionType {
	[SerializationData("EXPRESSION_SRC_PARAM")] Param,
	[SerializationData("EXPRESSION_SRC_CONST")] Const,
	[SerializationData("EXPRESSION_SRC_FUNCTION")] Function,
	[SerializationData("EXPRESSION_SRC_COMPLEX")] Complex
}