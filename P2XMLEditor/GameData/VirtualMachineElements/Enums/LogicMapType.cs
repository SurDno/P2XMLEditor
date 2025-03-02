using System.ComponentModel;
using P2XMLEditor.Attributes;

namespace P2XMLEditor.GameData.VirtualMachineElements.Enums;

[TypeConverter(typeof(EnumConverter))]
public enum LogicMapType {
	[SerializationData("LOGIC_MAP_TYPE_GLOBAL_MINDMAP")] Global,
	[SerializationData("LOGIC_MAP_TYPE_LOCAL_MINDMAP")] Local
}