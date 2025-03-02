using System.ComponentModel;
using P2XMLEditor.Attributes;

namespace P2XMLEditor.GameData.VirtualMachineElements.Enums;

[TypeConverter(typeof(EnumConverter))]
public enum ConditionType {
	[SerializationData("CONDITION_TYPE_CONST_TRUE")] ConstTrue,
	[SerializationData("CONDITION_TYPE_CONST_FALSE")] ConstFalse,
	[SerializationData("CONDITION_TYPE_VALUE_LESS")] ValueLess,
	[SerializationData("CONDITION_TYPE_VALUE_LESS_EQUAL")] ValueLessEqual,
	[SerializationData("CONDITION_TYPE_VALUE_LARGER")] ValueLarger,
	[SerializationData("CONDITION_TYPE_VALUE_LARGER_EQUAL")] ValueLargerEqual,
	[SerializationData("CONDITION_TYPE_VALUE_EQUAL")] ValueEqual,
	[SerializationData("CONDITION_TYPE_VALUE_NOT_EQUAL")] ValueNotEqual,
	[SerializationData("CONDITION_TYPE_VALUE_EXPRESSION")] ValueExpression
}