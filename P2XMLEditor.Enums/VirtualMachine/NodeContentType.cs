using System.ComponentModel;

namespace P2XMLEditor.Enums.VirtualMachine;

[TypeConverter(typeof(EnumConverter)), SerializationEnum]
public enum NodeContentType {
	[SerializationData("NODE_CONTENT_TYPE_INFO")] Info,
	[SerializationData("NODE_CONTENT_TYPE_FAILURE")] Failure,
	[SerializationData("NODE_CONTENT_TYPE_SUCCESS")] Success,
	[SerializationData("NODE_CONTENT_TYPE_KNOWLEDGE")] Knowledge
}