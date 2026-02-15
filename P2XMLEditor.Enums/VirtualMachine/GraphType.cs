using System.ComponentModel;

namespace P2XMLEditor.Enums.VirtualMachine;

[TypeConverter(typeof(EnumConverter)), SerializationEnum]
public enum GraphType {
	[SerializationData("GRAPH_TYPE_EVENTGRAPH")] EventGraph,
	[SerializationData("GRAPH_TYPE_PROCEDURE")] Procedure,
	[SerializationData("GRAPH_TYPE_TRADE")] Trade,
	[SerializationData("GRAPH_TYPE_ALL")] All
}