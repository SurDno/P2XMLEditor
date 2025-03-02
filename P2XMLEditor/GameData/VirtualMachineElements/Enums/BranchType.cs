using System.ComponentModel;
using P2XMLEditor.Attributes;

namespace P2XMLEditor.GameData.VirtualMachineElements.Enums;

[TypeConverter(typeof(EnumConverter))]
public enum BranchType {
	[SerializationData("STATE_TYPE_CASE_BRANCH")] Case,
	[SerializationData("STATE_TYPE_FLIPFLOP_BRANCH")] FlipFlop,
	[SerializationData("STATE_TYPE_MINVALUE_BRANCH")] MinValue,
	[SerializationData("STATE_TYPE_MAXVALUE_BRANCH")] MaxValue,
	[SerializationData("STATE_TYPE_MESSAGE_CAST_BRANCH")] MessageCast,
}

/* TODO: Figure out if following types are needed (never used):
	[SerializationData("STATE_TYPE_STATE")] STATE_TYPE_STATE,
	[SerializationData("STATE_TYPE_SPEECH")] STATE_TYPE_SPEECH,
	[SerializationData("STATE_TYPE_GRAPH")] STATE_TYPE_GRAPH
*/