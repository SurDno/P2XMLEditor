using System.ComponentModel;
using P2XMLEditor.Attributes;

namespace P2XMLEditor.GameData.VirtualMachineElements.Enums;

[TypeConverter(typeof(EnumConverter))]
[SerializationEnum]
public enum GraphType {
	[SerializationData("GRAPH_TYPE_EVENTGRAPH")] EventGraph,
	[SerializationData("GRAPH_TYPE_PROCEDURE")] Procedure,
	[SerializationData("GRAPH_TYPE_TRADE")] Trade,
	[SerializationData("GRAPH_TYPE_ALL")] All
}