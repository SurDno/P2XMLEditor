using System.ComponentModel;

namespace P2XMLEditor.Enums.VirtualMachine;

[TypeConverter(typeof(EnumConverter)), SerializationEnum]
public enum ActionLineType {
	[SerializationData("ACTION_LINE_TYPE_COMMON")] Common,
	[SerializationData("ACTION_LINE_TYPE_LOOP")] Loop,
	[SerializationData("ACTION_LINE_TYPE_INVENTORY")] Inventory,
	[SerializationData("ACTION_LINE_TYPE_MARKET")] Market,
	[SerializationData("ACTION_LINE_TYPE_GATE_SYSTEM")] GateSystem,
	[SerializationData("ACTION_LINE_TYPE_CUSTOM_GROUP")] CustomGroup
}