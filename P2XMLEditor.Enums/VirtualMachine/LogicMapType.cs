using System.ComponentModel;

namespace P2XMLEditor.Enums.VirtualMachine;

[TypeConverter(typeof(EnumConverter)), SerializationEnum]
public enum LogicMapType {
	[SerializationData("LOGIC_MAP_TYPE_GLOBAL_MINDMAP")] Global,
	[SerializationData("LOGIC_MAP_TYPE_LOCAL_MINDMAP")] Local
}