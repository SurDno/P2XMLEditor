using System.ComponentModel;

namespace P2XMLEditor.Enums.VirtualMachine;

[TypeConverter(typeof(EnumConverter)), SerializationEnum]
public enum LogicMapNodeType {
	[SerializationData("LM_NODE_TYPE_INITIAL")] Initial,
	[SerializationData("LM_NODE_TYPE_COMMON")] Common,
	[SerializationData("LM_NODE_TYPE_CONCLUSION")] Conclusion,
	[SerializationData("LM_NODE_TYPE_MISSION")] Mission
}