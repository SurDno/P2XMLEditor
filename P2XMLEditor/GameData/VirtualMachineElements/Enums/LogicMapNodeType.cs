using System.ComponentModel;
using P2XMLEditor.Attributes;

namespace P2XMLEditor.GameData.VirtualMachineElements.Enums;

[TypeConverter(typeof(EnumConverter))]
public enum LogicMapNodeType {
	[SerializationData("LM_NODE_TYPE_INITIAL")] Initial,
	[SerializationData("LM_NODE_TYPE_COMMON")] Common,
	[SerializationData("LM_NODE_TYPE_CONCLUSION")] Conclusion,
	[SerializationData("LM_NODE_TYPE_MISSION")] Mission
}