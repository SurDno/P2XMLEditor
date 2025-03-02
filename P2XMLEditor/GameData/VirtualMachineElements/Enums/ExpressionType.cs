using System.ComponentModel;
using P2XMLEditor.Attributes;

namespace P2XMLEditor.GameData.VirtualMachineElements.Enums;

[TypeConverter(typeof(EnumConverter))]
public enum ExpressionType {
	[SerializationData("EXPRESSION_SRC_PARAM")] Param,
	[SerializationData("EXPRESSION_SRC_CONST")] Const,
	[SerializationData("EXPRESSION_SRC_FUNCTION")] Function,
	[SerializationData("EXPRESSION_SRC_COMPLEX")] Complex
}